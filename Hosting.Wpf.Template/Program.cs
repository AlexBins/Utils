using System.Windows;
using Hosting.Wpf.Template.Boilerplate;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Wpf.Extensions.Hosting;

namespace Hosting.Wpf.Template;

public class Program
{
    public static async Task<int> Main(string[] args)
    {
        var builder = new HostBuilder()
            .ConfigureWpf<App>((ctx, svc) =>
            {
                svc.AddSingleton<Window, MainWindow>();
            })
            .UseSerilog((ctx, svc, config) =>
                config
                .WriteTo.Debug()
                .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{SourceContext}]{NewLine}{Message:lj}{NewLine}{Exception}")
                .Enrich.FromLogContext()
                .MinimumLevel.Debug());

        var host = builder.Build();
        var logger = host.Services.GetRequiredService<ILogger<Program>>();
        var applicationResult = host.Services.GetRequiredService<IApplicationResult>();
        try
        {
            await host.RunAsync();
            return applicationResult.ExitCode ?? 0;
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Host terminated unexpectedly");
            return -1;
        }
    }
}
