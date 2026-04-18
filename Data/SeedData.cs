// Data/SeedData.cs - CORRECTED VERSION
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace semproject.Data
{
    public static class SeedData
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();
            var configuration = serviceProvider.GetRequiredService<IConfiguration>(); // ADD THIS

            // Create roles if they don't exist
            string[] roleNames = { "Admin", "User" };

            foreach (var roleName in roleNames)
            {
                var roleExist = await roleManager.RoleExistsAsync(roleName);
                if (!roleExist)
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // Create admin user if it doesn't exist
            // CORRECT: Get from configuration
            var adminEmail = configuration["Admin:Email"] ?? "admin@example.com";
            var adminPassword = configuration["Admin:Password"] ?? "Admin123@";

            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                adminUser = new IdentityUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true
                };

                var createAdmin = await userManager.CreateAsync(adminUser, adminPassword);
                if (createAdmin.Succeeded)
                {
                    // Assign admin role
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                    Console.WriteLine("Admin user created successfully!");
                }
                else
                {
                    Console.WriteLine($"Error creating admin user: {string.Join(", ", createAdmin.Errors.Select(e => e.Description))}");
                }
            }
            else
            {
                // Ensure admin has Admin role
                if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                    Console.WriteLine("Admin role assigned to existing user!");
                }
            }
        }
    }
}