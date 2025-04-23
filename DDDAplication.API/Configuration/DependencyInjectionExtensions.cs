using System.Reflection;

namespace DDDAplication.API.Configuration
{
    public static class DependencyInjectionExtensions
    {
        /// <summary>
        /// Automatically registers all non-abstract classes from the specified assembly that implement interfaces,
        /// binding them to their corresponding interfaces in the dependency injection container with a scoped lifetime.
        /// </summary>
        /// <param name="services">The IServiceCollection to add the services to.</param>
        /// <param name="assembly">The assembly to scan for services.</param>
        /// <returns>The updated IServiceCollection.</returns>
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
