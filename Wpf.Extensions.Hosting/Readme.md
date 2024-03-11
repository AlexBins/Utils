# Hosting.Wpf

## What is it used for?
This library adds WPF support to [Microsoft.Extensions.Hosting](https://www.nuget.org/packages/Microsoft.Extensions.Hosting).

## How do I use it?
You configure your host to use WPF and add Window instances as required.
Then you use the provided extension method to run the application.

    class Program {
      
      static int Main(string[] args) {
        var builder = new HostBuilder()
            .AddWpf<App>()
            .UseSerilog((ctx, svc, config) =>
                config
                .WriteTo.Console()
                .Enrich.FromLogContext()
                .MinimumLevel.Debug())
            .ConfigureServices((ctx, svc) =>
            {
                // Services registered as Window will be shown on app start
                svc.AddSingleton<Window, MainWindow>();
            });

        var host = builder.Build();
        var logger = host.Services.GetRequiredService<ILogger<Program>>();
        try
        {
            await host.RunAsync();
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Host terminated unexpectedly");
            return -1;
        }
        return 0;
      }
    }