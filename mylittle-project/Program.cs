using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using mylittle_project.Application.Interfaces;
using mylittle_project.Domain.Entities;
using mylittle_project.infrastructure.Data;
using mylittle_project.infrastructure.Services;
using mylittle_project.Infrastructure.Repositories;
using mylittle_project.Infrastructure.Services;
using Scalar.AspNetCore;
using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// ─────────────────────────────────────────────
// 1) Swagger / Scalar
// ─────────────────────────────────────────────
builder.Services.AddOpenApi();

// ─────────────────────────────────────────────
// 2) DbContext
// ─────────────────────────────────────────────
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        b => b.MigrationsAssembly("mylittle-project.infrastructure"))
);

// ─────────────────────────────────────────────
// 3) Identity Setup
// ─────────────────────────────────────────────
builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.Password.RequireDigit = true;
        options.Password.RequiredLength = 8;
        options.SignIn.RequireConfirmedEmail = true;
    })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

// ─────────────────────────────────────────────
// 4) JWT Authentication
// ─────────────────────────────────────────────
var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]!);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidateAudience = true,
        ValidAudience = jwtSettings["Audience"],
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        RoleClaimType = ClaimTypes.Role,
        ClockSkew = TimeSpan.Zero
    };
});

// ─────────────────────────────────────────────
// 5) Domain Services
// ─────────────────────────────────────────────
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IDealerService, DealerService>();
builder.Services.AddScoped<ITenantService, TenantService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<ITenantPortalLinkService, TenantPortalLinkService>();
builder.Services.AddScoped<IKycService, KycService>();
builder.Services.AddScoped<ITenantSubscriptionService, TenantSubscriptionService>();
builder.Services.AddScoped<IGlobalSubscriptionService, GlobalSubscriptionService>();
builder.Services.AddScoped<IUserDealerService, UserDealerService>();
builder.Services.AddScoped<IVirtualNumberService, VirtualNumberService>();
builder.Services.AddScoped<IBuyerService, BuyerService>();
builder.Services.AddScoped<IFilterService, FilterService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IFeatureAccessService, FeatureAccessService>();
builder.Services.AddScoped<IDealerPlanAssignmentService, DealerPlanAssignmentService>();
builder.Services.AddScoped<IDealerSubscriptionApplicationService, DealerSubscriptionApplicationService>();
builder.Services.AddScoped<IProductReviewService, ProductReviewService>();
builder.Services.AddScoped<IBrandService, BrandService>();
builder.Services.AddScoped<IProductTagService, ProductTagService>();
builder.Services.AddScoped<IProductAttributeService, ProductAttributeService>();
builder.Services.AddTransient<IEmailSender, EmailSendService>();

// ─────────────────────────────────────────────
// 6) Controllers – JSON loop prevention
// ─────────────────────────────────────────────
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});

// ─────────────────────────────────────────────
// 7) Accessor & Client
// ─────────────────────────────────────────────
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient();

// ─────────────────────────────────────────────
// 8) CORS
// ─────────────────────────────────────────────
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
        builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});

// ─────────────────────────────────────────────
// 9) Build App
// ─────────────────────────────────────────────
var app = builder.Build();

// ─────────────────────────────────────────────
// 10) Migrate & Seed Roles
// ─────────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var ctx = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await ctx.Database.MigrateAsync();

    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    await RoleSeeder.SeedAsync(roleManager); // Implement this to seed roles
}

// ─────────────────────────────────────────────
// 11) Product Resync for Tenants
// ─────────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var productService = scope.ServiceProvider.GetRequiredService<IProductService>();
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    var tenantIds = await dbContext.Products
        .Select(p => p.TenantId)
        .Distinct()
        .ToListAsync();

    foreach (var tenantId in tenantIds)
    {
        Console.WriteLine($"🔄 Starting Resync for Tenant: {tenantId}");
        await productService.ResyncProductFieldValuesAsync(tenantId);
    }
}

// ─────────────────────────────────────────────
// 12) Swagger / Scalar
// ─────────────────────────────────────────────
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

// ─────────────────────────────────────────────
// 13) Middleware Pipeline
// ─────────────────────────────────────────────
app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
