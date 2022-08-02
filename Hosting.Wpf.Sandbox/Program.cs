using System.Windows.Controls;
using System.Windows.Media;
using Hosting.Wpf.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

class Program
{
    [STAThread]
    static int Main(string[] args)
    {
        var hostBuilder = new HostBuilder();
        hostBuilder.ConfigureWpf(svc => svc.AddSingleton(
            _ => new Page
            {
                Title = "Test Title",
                Background = Brushes.Blue,
            }));
        return hostBuilder.RunWpf(CancellationToken.None);
    }
}