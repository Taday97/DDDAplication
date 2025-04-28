using DDDAplication.Domain.Entities;
using DDDAplication.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging; 
using DDDAplication.Infrastructure.Helpers;


namespace DDDAplication.Infrastructure.Data
{
    public class AppDbContextSeed
    {
        public static async Task SeedDatabaseAsync(AppDbContext context, UserManager<ApplicationUser> userManager, RoleManager<Role> roleManager, IConfiguration configuration, ILogger<AppDbContextSeed> logger, JwtService jwtService)
        {
            foreach (var roleName in Enum.GetNames(typeof(RoleEnum)))
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    var role = new Role{
                      Name= roleName
                    };

                    if (roleName.Equals(RoleEnum.Admin.ToString()) || roleName.Equals(RoleEnum.Developer.ToString()))
                    {
                        Role newRol = new Role
                        {
                            Name = roleName,
                        };

                        var stamp =  jwtService.GenerateRoleToken(newRol);
                        role.ConcurrencyStamp = stamp;
                    }

                    var result = await roleManager.CreateAsync(role);
                    if (!result.Succeeded)
                    {
                        logger.LogError("Error creating role {RoleName}: {Errors}", roleName, string.Join(", ", result.Errors.Select(e => e.Description)));
                    }
                }
            }

            if (!userManager.Users.Any())
            {
                var adminUser = new ApplicationUser { UserName = "admin", Email = "admin@admin.com", EmailConfirmed = true };
                var developerUser = new ApplicationUser { UserName = "developer", Email = "developer@admin.com", EmailConfirmed = true };
                var testUser = new ApplicationUser { UserName = "test", Email = "test@admin.com", EmailConfirmed = true };
              
                await userManager.CreateAsync(adminUser, "Admin123*");
                await userManager.CreateAsync(developerUser, "Developer123*");
                await userManager.CreateAsync(testUser, "Test123*");

            
                await userManager.AddToRoleAsync(adminUser, RoleEnum.Admin.ToString());
                await userManager.AddToRoleAsync(developerUser, RoleEnum.Developer.ToString());
            }


            await context.SaveChangesAsync();
        }
    }
}
