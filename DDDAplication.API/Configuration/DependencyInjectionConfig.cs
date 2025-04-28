using DDDAplication.Application;
using DDDAplication.Infrastructure.Helpers;
using System.Reflection;

namespace DDDAplication.API.Configuration
{
    public static class DependencyInjectionConfig
    {
        /// <summary>
        /// Automatically registers all services defined in the DDDAplication.Application and DDDAplication.Infrastructure assemblies 
        /// into the dependency injection container.
        /// </summary>
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {

            var applicationAssembly = Assembly.Load("DDDAplication.Application");
            services.AddServicesFromAssembly(applicationAssembly);

            var infrastructureAssembly = Assembly.Load("DDDAplication.Infrastructure");
            services.AddServicesFromAssembly(infrastructureAssembly);

            services.AddScoped<JwtService>();

            return services;
        }
    }
}