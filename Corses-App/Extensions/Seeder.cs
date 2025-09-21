namespace Corses_App.Extensions
{
    using Courses_App.Core.Models;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.Extensions.DependencyInjection;
    using System.IO;

    public static class Seeder
    {
        public static async Task SeedRolesAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            string[] roles = { "User", "Admin", "Instructor" };
            foreach (var role in roles)
            {
                var roleExist = await roleManager.RoleExistsAsync(role);
                if (!roleExist)
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }
        }
        public static async Task SeedFoldersAsync(IServiceProvider serviceProvider)
        {
            var env = serviceProvider.GetRequiredService<IWebHostEnvironment>();
            string[] folders =
                 {
                        Path.Combine(env.WebRootPath, "img", "profilePicture"),
                        Path.Combine(env.WebRootPath, "img", "categories"),
                        Path.Combine(env.WebRootPath, "img", "CoursesImages"),

                        Path.Combine(env.WebRootPath, "Videos")
                    };
            string defultPicture = Path.Combine(env.WebRootPath, "img", "profilePicture", "profileImage.jpg");
            
            foreach (var folder in folders)
            {
                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }
            }
           

            await Task.CompletedTask;
        }
        public static async Task SeedAdminAsync(IServiceProvider serviceProvider)
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<User>>();
            string email = "admin@admin.com";
            string passowrd = "Admin@admin12";
            string fullName = "Hamza Salem";
            var adminUser = await userManager.FindByEmailAsync(email.ToUpper());
            if (adminUser == null)
            {
                var user = new User()
                {
                    Email = email,
                    FullName = fullName,
                    EmailConfirmed = true,
                    UserName = email

                };
                var result = await userManager.CreateAsync(user, passowrd);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, "Admin");
                }
            }
        }

    }
}
