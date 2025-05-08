using System.Windows;
using AlexBins.Wpf.Navigation;
using AlexBins.Wpf.Navigation.Extensions;
using CommunityToolkit.Mvvm.DependencyInjection;
using Dapplo.Microsoft.Extensions.Hosting.Wpf;

namespace TestWpfApp;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window, IWpfShell
{
    public MainWindow()
    {
        InitializeComponent();

        Loaded += async delegate
        {
            try
            {
                var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
                var ui = Ioc.Default.GetRequiredService<IUiProxy>();
                var nav = Ioc.Default.GetRequiredService<INavigationService>();
                await ui.InitializeAsync(Dispatcher, cts.Token);
                await nav.RegisterOutletAsync(INavigationOutlet.Default, LoadContent, cts.Token);
            }
            catch (Exception ex)
            {
                // Handle exceptions as needed
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        };
    }

    private void LoadContent(object? content)
    {
        try
        {
            Content = content;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"An error occurred while loading content: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}