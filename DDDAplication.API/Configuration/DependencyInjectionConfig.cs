using DDDAplication.Application;
using System.Reflection;

namespace DDDAplication.API.Configuration
{
    public static class DependencyInjectionConfig
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {

            var applicationAssembly = Assembly.Load("DDDAplication.Application");
            services.AddServicesFromAssembly(applicationAssembly);

            var infrastructureAssembly = Assembly.Load("DDDAplication.Infrastructure");
            services.AddServicesFromAssembly(infrastructureAssembly);

            return services;
        }
    }
}