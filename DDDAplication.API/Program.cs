using DDDAplication.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

public class Program
{
    public static async Task Main(string[] args)
    {
        var host = CreateHostBuilder(args).Build();

        using (var scope = host.Services.CreateScope())
        {
            // Get the dbcontext instance
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            // do the migration async
            await dbContext.Database.MigrateAsync();
          
        }

        await host.RunAsync();
    }
    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>(); 
            });
}
