using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using mylittle_project.Application.Interfaces;
using mylittle_project.Application.Interfaces.Repositories;
using mylittle_project.Domain.Entities;
using mylittle_project.infrastructure.Data;
using mylittle_project.infrastructure.Services;
using mylittle_project.Infrastructure.Repositories;
using mylittle_project.Infrastructure.Services; // for EmailSender
using Scalar.AspNetCore;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// ─────────────────────────────────────────────────────────────
// 1) Swagger / Scalar (API Docs)
// ─────────────────────────────────────────────────────────────
builder.Services.AddOpenApi();

// ─────────────────────────────────────────────────────────────
// 2) EF Core – AppDbContext
// ─────────────────────────────────────────────────────────────
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        b => b.MigrationsAssembly("mylittle-project.infrastructure"))
);

// ─────────────────────────────────────────────────────────────
// 3) Identity with Cookie Auth
// ─────────────────────────────────────────────────────────────
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 6;
    options.SignIn.RequireConfirmedEmail = true;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

// ⚠️ Do NOT add AddAuthentication().AddCookie() again — already registered by Identity

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.ExpireTimeSpan = TimeSpan.FromHours(2);
    options.SlidingExpiration = true;

    // Prevent redirect in APIs
    options.Events.OnRedirectToLogin = context =>
    {
        context.Response.StatusCode = 401;
        return Task.CompletedTask;
    };
    options.Events.OnRedirectToAccessDenied = context =>
    {
        context.Response.StatusCode = 403;
        return Task.CompletedTask;
    };
});

// ─────────────────────────────────────────────────────────────
// 4) EmailSender for 2FA, Email Confirmation, etc.
// ─────────────────────────────────────────────────────────────
builder.Services.AddTransient<IEmailSender, EmailSender>();

// ─────────────────────────────────────────────────────────────
// 5) App Services and Repositories
// ─────────────────────────────────────────────────────────────
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddScoped<ITenantService, TenantService>();
builder.Services.AddScoped<IDealerService, DealerService>();
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
builder.Services.AddScoped<ITenantPlanAssignmentService, TenantPlanAssignmentService>();
builder.Services.AddScoped<IDealerSubscriptionService, DealerSubscriptionService>();
builder.Services.AddScoped<IProductReviewService, ProductReviewService>();
builder.Services.AddScoped<IBrandService, BrandService>();
builder.Services.AddScoped<IProductTagService, ProductTagService>();
builder.Services.AddScoped<IProductAttributeService, ProductAttributeService>();
builder.Services.AddTransient<IEmailSender, EmailSender>();

builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient();

// ─────────────────────────────────────────────────────────────
// 6) Controllers
// ─────────────────────────────────────────────────────────────
builder.Services.AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles
    );

// ─────────────────────────────────────────────────────────────
// 7) Build and Migrate
// ─────────────────────────────────────────────────────────────
var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
    await SeedFeatures.RunAsync(db);
}

// ─────────────────────────────────────────────────────────────
// 8) Swagger (Only in Dev)
// ─────────────────────────────────────────────────────────────
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

// ─────────────────────────────────────────────────────────────
// 9) Middleware
// ─────────────────────────────────────────────────────────────
app.UseHttpsRedirection();
app.UseAuthentication(); // 👈 Must come before UseAuthorization
app.UseAuthorization();
app.MapControllers();
app.Run();
