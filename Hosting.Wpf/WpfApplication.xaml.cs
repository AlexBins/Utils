namespace Hosting.Wpf;
using System.Windows;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
internal partial class WpfApplication
{
    private readonly PageHost _pageHost;
    public WpfApplication(PageHost pageHost)
    {
        _pageHost = pageHost;
        this.InitializeComponent();
    }
    private void App_OnStartup(object sender, StartupEventArgs e)
    {
        _pageHost.Show();
    }

    private void App_OnExit(object sender, ExitEventArgs e)
    {
        
    }
}
