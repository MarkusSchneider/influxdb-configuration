using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Serilog;

namespace IOBroker.Aggregation;

internal class Program
{
    public static async Task Main(string[] args)
    {
        Serilog.Debugging.SelfLog.Enable(msg => Console.WriteLine(msg));
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .CreateLogger();

        try
        {
            var hostBuilder = CreateHostBuilder(args);

            await hostBuilder.RunConsoleAsync();
        }
        catch (Exception exception)
        {
            Log.Fatal(exception, $"{typeof(Program)}: Application startup failed");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    private static IHostBuilder CreateHostBuilder(string[] args)
    {
        return Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((hostBuilderContext, configurationBuilder) =>
            {
                var environment = hostBuilderContext.HostingEnvironment;

                configurationBuilder.SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", false, true)
                    .AddJsonFile($"appsettings.{environment.EnvironmentName}.json", true, true)
                    .AddEnvironmentVariables()
                    .AddCommandLine(args);
            })
            .UseSerilog((hostBuilderContext, loggerConfiguration) =>
            {
                loggerConfiguration.MinimumLevel.Debug();
                loggerConfiguration.WriteTo.Console();
            })
            .ConfigureServices((hostBuilderContext, services) =>
            {
                services.AddHostedService<InfluxAggregationService>();
                services.Configure<InfluxOptions>(hostBuilderContext.Configuration.GetSection(InfluxOptions.SectionName));
            });
    }
}
