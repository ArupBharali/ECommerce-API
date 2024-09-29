using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Serilog;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Prometheus;

public class Program
{
    public static void Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.Console()
            .WriteTo.File("logs/ECommerceApiGatewayLog.txt", rollingInterval: RollingInterval.Day)
            .CreateLogger();

        try
        {
            Log.Information("Starting API Gateway");
            CreateHostBuilder(args).Build().Run();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Application start-up failed");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .UseSerilog() // Use Serilog
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder
                .ConfigureServices(s => s.AddSingleton(webBuilder))
                    .ConfigureAppConfiguration(
                          ic => ic.AddJsonFile(Path.Combine("",
                                                            "ocelot.json")))
                    .UseStartup<Startup>()
                    .UseUrls("http://*:5000") // Set the port here;
                    ;
            });
}

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddCors(options =>
        {
            //policy 1 - any method,any header
            options.AddPolicy("CorsPolicy",
                builder =>
                {
                    builder.WithOrigins("http://localhost:5173", "http://localhost:7173") // Replace with your React app's URL
                   .AllowAnyMethod()
                   .AllowAnyHeader()
                   //.WithExposedHeaders("Authorization")
                   .AllowCredentials();
                });

            // Policy 2 - WithMethods("GET", "POST"), any header, expose header ("Authorization")
            options.AddPolicy("Origin2Policy", builder =>
            {
                builder.WithOrigins("http://another-trusted-origin.com") // Replace with your second origin
                       .WithMethods("GET", "POST") // Specify allowed methods for this origin
                       .AllowAnyHeader()
                       .WithExposedHeaders("Authorization")
                       .AllowCredentials();
            });
        });
        services.AddOcelot();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseRouting();
        app.UseCors("CorsPolicy");

        // Enable Prometheus metrics
        app.UseMetricServer(); // Exposes /metrics endpoint
        app.UseHttpMetrics();  // Collects metrics for HTTP requests

        //app.UseEndpoints(endpoints =>
        //{
        //    endpoints.MapControllers();
        //});

        app.UseOcelot().Wait();
    }
}
