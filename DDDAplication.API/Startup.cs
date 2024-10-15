using DDDAplication.API.Extensions;
using DDDAplication.Application;
using DDDAplication.Domain.Entities;
using DDDAplication.Infrastructure.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

public class Startup
{
    public IConfiguration Configuration { get; }

    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        // Configuration DbContext con Identity
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

        // Configuration Identity
        services.AddIdentity<User, Rol>(options =>
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

        // Configuration JWT Authentication
        var jwtSettings = Configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["Secret"];

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings["Issuer"],
                ValidAudience = jwtSettings["Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
            };
        });

      

        services.AddAuthorization();

        // Add services to the container.
        services.AddControllers();

        services.AddEndpointsApiExplorer();
        services.AddSwagger();//Swagger

        // Register application and infrastructure layers
        services.AddApplication(); // Register the application layer
        services.AddDataAccess(Configuration); // Register the infrastructure layer
        //services.AddJwt(Configuration);// Token Configurations
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

        app.UseAuthentication(); // Authentication
        app.UseAuthorization();  // Middleware to Authorization

        // Configura los puntos finales
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers(); // Mapea los controladores
        });

        
    }
}
