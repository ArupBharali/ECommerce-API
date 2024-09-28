using ECommerceAPI.ECommerceUserAPI.Data;
using ECommerceAPI.ECommerceUserAPI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ECommerceAPI.ECommerceUserAPI.Data
{
    public static class DbInitializer
    {
        public static async Task InitializeUsers(ECommerceUserDbContext context, IServiceProvider serviceProvider)
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            // Create Admin role if it doesn't exist
            string[] roleNames = { "Admin", "Customer", "Manager", "Seller", "Guest", "Support Staff", "Analyst" };
            IdentityResult roleResult;

            foreach (var roleName in roleNames)
            {
                var roleExist = await roleManager.RoleExistsAsync(roleName);
                if (!roleExist)
                {
                    roleResult = await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // Seed users with multiple roles
            await SeedUser(userManager, roleManager, "admin@example.com", "Admin@123!", new[] { "Admin", "Manager" });
            await SeedUser(userManager, roleManager, "manager@example.com", "Manager@123!", new[] { "Manager" });
            await SeedUser(userManager, roleManager, "seller@example.com", "Seller@123!", new[] { "Seller", "Customer" });
            await SeedUser(userManager, roleManager, "customer@example.com", "Customer@123!", new[] { "Customer" });
            await SeedUser(userManager, roleManager, "support@example.com", "Support@123!", new[] { "Support Staff" });

        }
        private static async Task SeedUser(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, string email, string password, string[] roles)
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user == null)
            {
                user = new ApplicationUser()
                {
                    UserName = email,
                    Email = email
                };
                await userManager.CreateAsync(user, password);
            }

            foreach (var role in roles)
            {
                if (!await userManager.IsInRoleAsync(user, role))
                {
                    await userManager.AddToRoleAsync(user, role);
                }
            }
        }
    }
}
