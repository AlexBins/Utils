namespace Hosting.Wpf;

using System.Collections.Generic;
using System.Linq;
using System.Windows;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
internal partial class WpfApplication
{
    private Window[] _windows;
    public WpfApplication(IEnumerable<Window> windows)
    {
        _windows = windows.ToArray();
        this.InitializeComponent();
    }
    private void App_OnStartup(object sender, StartupEventArgs e)
    {
        foreach (var window in _windows)
        {
            window.Show();
        }
    }

    private void App_OnExit(object sender, ExitEventArgs e)
    {
        foreach (Window window in Windows)
        {
            window.Close();
        }
    }
}
