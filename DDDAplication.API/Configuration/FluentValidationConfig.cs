using FluentValidation.AspNetCore;
using FluentValidation;
using DDDAplication.Application.Validators;

namespace DDDAplication.API.Configuration
{
    public static class FluentValidationConfig
    {
        public static void AddFluentValidationServices(this IServiceCollection services)
        {

            var domainAssembly = typeof(RegisterModelDtoValidator).Assembly;
            services.AddValidatorsFromAssembly(domainAssembly);


            services.AddFluentValidationAutoValidation();
            services.AddFluentValidationClientsideAdapters();
        }
    }
}
