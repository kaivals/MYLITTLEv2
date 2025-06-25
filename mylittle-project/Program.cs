using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using mylittle_project.Application.Interfaces;
using mylittle_project.infrastructure.Data;
using mylittle_project.infrastructure.Services;
using mylittle_project.Infrastructure.Services;
using mylittle_project.Domain.Entities;          // for the seeder
using System.Text.Json.Serialization;           // for ref-loop handling

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
builder.Services.AddScoped<IBusinessService, BusinessService>();
builder.Services.AddScoped<ITenantService, TenantService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<ITenantPortalLinkService, TenantPortalLinkService>();
builder.Services.AddScoped<IKycService, KycService>();
builder.Services.AddScoped<ISubscriptionService, SubscriptionService>();
builder.Services.AddScoped<IUserDealerService, UserDealerService>();
builder.Services.AddScoped<IVirtualNumberService, VirtualNumberService>();
builder.Services.AddScoped<IBuyerService, BuyerService>();

builder.Services.AddHttpClient();

// ─────────────────────────────────────────────────────────────
// 4)  Controllers – add JSON reference-loop protection
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
// 6)  Apply migrations + seed master Modules/Features
//     (run once per environment; SeedFeatures exits if data exists)
// ─────────────────────────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var ctx = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    // Apply any pending migrations
    await ctx.Database.MigrateAsync();

    // Seed initial FeatureModules & Features (safe if already seeded)
    await SeedFeatures.RunAsync(ctx);
}

// ─────────────────────────────────────────────────────────────
// 7)  Swagger / Scalar
// ─────────────────────────────────────────────────────────────
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

// ─────────────────────────────────────────────────────────────
// 8)  Middleware pipeline
// ─────────────────────────────────────────────────────────────
app.UseHttpsRedirection();
app.MapControllers();

app.Run();
