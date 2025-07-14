using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mylittle_project.Domain.Entities
{
    public static class RoleSeeder
    {
        private static readonly string[] Roles = { "Admin", "TenantOwner", "StoreManager", "Dealer", "Buyer", "Guest" };
        public static async Task SeedAsync(RoleManager<IdentityRole> roleManager)
        {
            foreach (var role in Roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }
        }
    }

}
