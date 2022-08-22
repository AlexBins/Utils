using System.Windows;
using Hosting.Wpf.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

class Program
{
    [STAThread]
    static int Main(string[] args)
    {
        var hostBuilder = new HostBuilder();
        hostBuilder.ConfigureAppConfiguration(config =>
        {
            config.AddCommandLine(args);
            config.AddJsonFile("appsettings.json");
        });
        hostBuilder.ConfigureLogging(logging => logging.AddConsole());
        hostBuilder.ConfigureWpf(svc =>
        {
            svc.AddSingleton(_ => new Window { Title = "My first Test Window" });
            svc.AddSingleton(_ => new Window { Title = "My second Test Window" });
        });
        return hostBuilder.RunWpf(CancellationToken.None);
    }
}