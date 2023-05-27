using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BuDiTest.Areas.Identity.Data
{
    public static class ContextSeed
    {
          
        public static async Task seedRolesAsync(RoleManager<IdentityRole> roleManager)
        {

            string[] roleNames = { "SuperAdmin", "Manager", "Employee" };

            foreach (var roleName in roleNames)
            {
                // Check if the role already exists
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    // Create the role if it doesn't exist
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

        }

        public static async Task SeedSuperAdminAsync(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            ApplicationUser defaultAdmin = new ApplicationUser
            {
                UserName = "SuperAdmin123@gmail.com",
                Email = "SuperAdmin123@gmail.com",
                EmailConfirmed = true,


            };
            if (userManager.Users.All(u => u.Id != defaultAdmin.Id))
            {
                var user = await userManager.CreateAsync(defaultAdmin, "SuperAdmin123@");

                if (user.Succeeded)
                {
                    await userManager.AddToRoleAsync(defaultAdmin, "SuperAdmin");

                }
            }

        }

    }
}
