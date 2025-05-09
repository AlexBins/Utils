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
        var outletRouteGroupings = navigationService
            .RouteConfiguration
            .GroupBy(route => route.Outlet)
            .ToArray();

        if (outletRouteGroupings.FirstOrDefault(grouping => grouping.Key == INavigationOutlet.Default) is { } defaultGrouping)
        {
            if (defaultGrouping.FirstOrDefault(route => route.IsHome) is { } defaultHome)
            {
                await navigationService.NavigateAsync($"/{defaultHome.Name}", cancellationToken);
            }
        }

        foreach (var outletRoutes in outletRouteGroupings.Where(grouping => grouping.Key != INavigationOutlet.Default))
        {
            if (outletRoutes.FirstOrDefault(route => route.IsHome) is { } home)
            {
                await navigationService.NavigateAsync($"{home.Name}", cancellationToken);
            }
        }
    }
}