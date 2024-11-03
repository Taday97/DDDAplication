using DDDAplication.Domain.Entities;
using DDDAplication.Domain.Enums;
using DDDAplication.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDDAplication.Infrastructure
{
    public static class AutomatedMigration
    {
        public static async Task MigrateAsync(IServiceProvider services, IConfiguration configuration)
        {
            var context = services.GetRequiredService<AppDbContext>();

            if (context.Database.IsSqlServer())  context.Database.Migrate();

            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = services.GetRequiredService<RoleManager<Role>>();
            var logger = services.GetRequiredService<ILogger<AppDbContextSeed>>();
            await AppDbContextSeed.SeedDatabaseAsync(context, userManager, roleManager, configuration, logger); 
        }
    }
}