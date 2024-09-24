using DDDAplication.Application; 
using DDDAplication.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

public class Startup
{
    public IConfiguration Configuration { get; }

    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public void ConfigureServices(IServiceCollection services)
    {
       
        // Add services to the container.
        services.AddControllers();

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

        // Register application and infrastructure layers
        services.AddApplication(); // Register the application layer
        services.AddDataAccess(Configuration); // Register the infrastructure layer
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {

        // Configure the HTTP request pipeline.
        if (env.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
            app.UseHttpsRedirection();
            app.UseStaticFiles(); // Solo si es necesario
        }
        else
        {
            app.UseExceptionHandler("/Home/Error");
            app.UseHsts();
        }

        app.UseRouting();

        app.UseCors(corsPolicyBuilder =>
            corsPolicyBuilder.AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader()
        );

        app.UseHttpsRedirection(); // Redirección a HTTPS

        app.UseAuthorization(); // Middleware de autorización

        // Configura los puntos finales
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers(); // Mapea los controladores
        });

        
    }
}
