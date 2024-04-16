using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ServiceWorker;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;
public class Program
{
    public static async Task Main(string[] args)
    {
        var host = Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
            })
            .ConfigureServices((hostContext, services) =>
    {
        // Register IMongoClient service
        services.AddSingleton<IMongoClient>(provider =>
        {
            var connectionString = "mongodb://localhost:27018/";
            return new MongoClient(connectionString);
        });

        services.AddControllers();
        services.AddHostedService<Worker>();
    })
            .Build();

        await host.RunAsync();
    }
}

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers();  // Add this line
        services.AddLogging();
        services.AddHostedService<Worker>();
    }


    public void Configure(IApplicationBuilder app)
    {
        app.UseRouting();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    }
}