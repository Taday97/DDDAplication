using System.Reflection;

namespace DDDAplication.API.Configuration
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddServicesFromAssembly(this IServiceCollection services, Assembly assembly)
        {
            var types = assembly.GetTypes();

            foreach (var type in types)
            {
                var interfaces = type.GetInterfaces();

                foreach (var @interface in interfaces)
                {
                    if (type.IsClass && @interface.IsAssignableFrom(type) && !type.IsAbstract)
                    {
                        services.AddScoped(@interface, type);
                    }
                }
            }

            return services;
        }
    }
}
