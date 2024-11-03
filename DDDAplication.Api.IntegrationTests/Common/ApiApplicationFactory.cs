using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using DDDAplication.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc.Testing;
using DDDAplication.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace DDDAplication.Api.IntegrationTests.Common
{
    public class ApiApplicationFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                // Remove the real database configuration
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                // Add an in-memory database for testing
                services.AddDbContext<AppDbContext>(options =>
                {
                    options.UseInMemoryDatabase("InMemoryDbForTesting");
                });

                // Configure Identity for use in tests
                services.AddIdentity<ApplicationUser, Role>(options =>
                {
                    options.Password.RequireDigit = true; // Requires at least one digit
                    options.Password.RequireLowercase = true; // Requires at least one lowercase letter
                    options.Password.RequireUppercase = false; // Set to false to not require uppercase letters
                    options.Password.RequireNonAlphanumeric = true; // Requires at least one special character
                    options.Password.RequiredLength = 6; // Minimum password length
                    options.Password.RequiredUniqueChars = 1; // Required unique characters
                })
                .AddEntityFrameworkStores<AppDbContext>()
                .AddDefaultTokenProviders();
            });
        }
    }
}