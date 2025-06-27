using mylittle_project.Domain.Entities;
using mylittle_project.infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace mylittle_project.infrastructure.Services
{
    public static class SeedFeatures
    {
        public static async Task RunAsync(AppDbContext ctx)
        {
            if (await ctx.FeatureModules.AnyAsync()) return;   // already seeded

            // 1) Categories Management
            var catModule = new FeatureModule
            {
                Id = Guid.NewGuid(),
                Key = "catalog",
                Name = "Catalog Management",
                Description = "Product and inventory management features",
                Features = new List<Feature>
            {
                    new() { Id = Guid.NewGuid(), Key = "products",      Name = "Products" },
                    new() { Id = Guid.NewGuid(), Key = "brands",        Name = "Brands" },
                    new() { Id = Guid.NewGuid(), Key = "reviews",       Name = "Reviews" },
                    new() { Id = Guid.NewGuid(), Key = "product-tags",  Name = "Product Tags", IsPremium = true },
                    new() { Id = Guid.NewGuid(), Key = "categories",    Name = "Categories", Description = "Manage product categories and filters", IsPremium = false },
                    new() { Id = Guid.NewGuid(), Key = "filters",       Name = "Filters", Description = "Manage filters assigned to product categories", IsPremium = false }

            }
            };

            // 2) Sales & Orders (etc.)
            var salesModule = new FeatureModule
            {
                Id = Guid.NewGuid(),
                Key = "sales",
                Name = "Sales & Orders",
                Description = "Order processing and sales management",
                Features = new List<Feature>
            {
                new() { Id = Guid.NewGuid(), Key = "orders",          Name = "Orders" },
                new() { Id = Guid.NewGuid(), Key = "discounts",       Name = "Discounts & Offers", IsPremium = true }
            }
            };



            // ...add other modules here...

            ctx.FeatureModules.AddRange(catModule, salesModule);
            await ctx.SaveChangesAsync();
        }
    }

}
