using Microsoft.EntityFrameworkCore;
using mylittle_project.Application.Interfaces;
using mylittle_project.Application.Interfaces.Repositories;
using mylittle_project.infrastructure.Data;
using mylittle_project.infrastructure.Services;
using mylittle_project.Infrastructure.Repositories;
using mylittle_project.Infrastructure.Services;
using Scalar.AspNetCore;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// ─────────────────────────────────────────────────────────────
// 1)  Swagger / Scalar
// ─────────────────────────────────────────────────────────────
builder.Services.AddOpenApi();

// ─────────────────────────────────────────────────────────────
// 2)  EF Core DbContext
// ─────────────────────────────────────────────────────────────
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        b => b.MigrationsAssembly("mylittle-project.infrastructure"))
);

// ─────────────────────────────────────────────────────────────
// 3)  Register domain services
// ─────────────────────────────────────────────────────────────
// Register the generic repository for all entities
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

builder.Services.AddScoped<IDealerService, DealerService>();
builder.Services.AddScoped<ITenantService, TenantService>();
builder.Services.AddScoped<IProductInterface, ProductService>();
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
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IProductReviewService, ProductReviewService>();
builder.Services.AddScoped<IBrandService, BrandService>();



// ✅ Register IHttpContextAccessor (required for tenant-based services)
builder.Services.AddHttpContextAccessor();

// Register HTTP Client
builder.Services.AddHttpClient();

// ─────────────────────────────────────────────────────────────
// 4)  Controllers – JSON ref-loop handling
// ─────────────────────────────────────────────────────────────
builder.Services
       .AddControllers()
       .AddJsonOptions(o =>
           o.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);

// ─────────────────────────────────────────────────────────────
// 5)  Build the app
// ─────────────────────────────────────────────────────────────
var app = builder.Build();

// ─────────────────────────────────────────────────────────────
// 6)  Apply migrations + seed FeatureModules/Features
// ─────────────────────────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var ctx = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    // Apply any pending migrations
    await ctx.Database.MigrateAsync();

    // Seed initial FeatureModules and Features (safe if already seeded)
    await SeedFeatures.RunAsync(ctx);
}

// ─────────────────────────────────────────────────────────────
// 7)  Swagger / Scalar (for dev only)
// ─────────────────────────────────────────────────────────────
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

// ─────────────────────────────────────────────────────────────
// 8)  Middleware and Routing
// ─────────────────────────────────────────────────────────────
app.UseHttpsRedirection();
app.MapControllers();

app.Run();
