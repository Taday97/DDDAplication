using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace DDDAplication.Application
{
    public static class ApplicationAutommaperConfig
    {
        public static void AddAutoMapperServices(this IServiceCollection services)
        {
            var assembly = Assembly.GetExecutingAssembly();
            services.AddAutoMapper(assembly);
        }
    }
}
