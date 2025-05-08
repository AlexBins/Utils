using AlexBins.Wpf.Navigation;
using AlexBins.Wpf.Navigation.Types;
using CommunityToolkit.Mvvm.DependencyInjection;
using Dapplo.Microsoft.Extensions.Hosting.Wpf;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace TestWpfApp;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = new HostBuilder()
            .ConfigureLogging(logging => logging.AddConsole())
            .ConfigureWpf(wpf => wpf.UseApplication<App>().UseWindow<MainWindow>())
            .ConfigureServices(svc => svc.AddNavigationServices()
                .AddRoute(new Route<Page1, Page1Vm>("page1", isHome: true))
                .AddRoute(new Route<Page2, Page2Vm>("page2")))
            .UseWpfLifetime();
        var host = builder.Build();
        Ioc.Default.ConfigureServices(host.Services);
        await host.StartAsync();
        await host.WaitForShutdownAsync();
    }
}