using Microsoft.Extensions.Hosting;

namespace AlexBins.Wpf.Navigation.Internal;

public class NavigationBackgroundService(
    INavigationService navigationService)
    : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await NavigateToHomeAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    private async Task NavigateToHomeAsync(CancellationToken cancellationToken)
    {
        var home = navigationService.RouteConfiguration.FirstOrDefault(route => route.IsHome);
        if (home is null)
        {
            return;
        }
        
        await navigationService.NavigateAsync($"/{home.Name}", cancellationToken);
    }
}