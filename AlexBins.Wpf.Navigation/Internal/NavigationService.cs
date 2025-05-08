using System.Collections.Immutable;
using System.Windows;
using AlexBins.Wpf.Navigation.Types;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AlexBins.Wpf.Navigation.Internal;

internal class NavigationService(
    ILogger<NavigationService> logger,
    IServiceProvider serviceProvider,
    IUiProxy uiProxy,
    NavigationServiceConfiguration configuration,
    IEnumerable<Route> routes)
    : INavigationService
{
    private readonly Dictionary<string, (INavigationOutlet Outlet, NavigationFrame? CurrentFrame)> _outlets = [];
    private List<NavigationFrame> _navigationStack = [];
    
    public IReadOnlyCollection<Route> RouteConfiguration { get; } = routes.ToImmutableArray();
    public string CurrentRoute => $"/{string.Join('/', _navigationStack.Select(frame => frame.ToRouteFragment()))}";
    
    public event EventHandler<NavigationEventArgs>? Navigated;
    public event EventHandler<NavigationEventArgs>? Navigating;

    public async Task RegisterOutletAsync(string name, INavigationOutlet outlet, CancellationToken cancel)
    {
        logger.LogInformation("Navigation Outlet {Outlet} registered", name);
        _outlets[name] = new ValueTuple<INavigationOutlet, NavigationFrame?>(outlet, null);

        using var cts = WithTimeout(cancel, out var timeoutToken);
        try
        {
            var currentFrame = _navigationStack.LastOrDefault(frame => frame.Outlet == name);
            if (currentFrame is not null)
            {
                await UpdateOutletAsync(currentFrame.Outlet, currentFrame, cts.Token);
            }
        }
        catch (OperationCanceledException) when (timeoutToken is { IsCancellationRequested: true })
        {
            logger.LogError("Initial navigation for outlet {Outlet} timed out after {Timeout}", name, configuration.NavigationTimeout);
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Initial navigation for outlet {Outlet} failed", name);
        }
    }

    public async Task NavigateAsync(string path, Dictionary<string, object>? parameters, CancellationToken cancel)
    {
        logger.LogInformation("Navigating to {Path}", path);
        using var cts = WithTimeout(cancel, out var timeoutToken);
        try
        {
            var (newStack, droppedFrames) =
                RouteAnalyzer.AppendRoute(_navigationStack, RouteConfiguration, path, parameters ?? []);
            var targetFrame = newStack.Count > 0 ? newStack[^1] : null;
            List<string> outletsToUpdate = [.. droppedFrames.Select(frame => frame.Outlet).Distinct()];
            if (targetFrame is not null && !outletsToUpdate.Contains(targetFrame.Outlet))
            {
                outletsToUpdate.Add(targetFrame.Outlet);
            }

            foreach (var outletFrames in newStack
                         .GroupBy(frame => frame.Outlet)
                         .Where(grouping => outletsToUpdate.Contains(grouping.Key)))
            {
                await UpdateOutletAsync(outletFrames.Key, outletFrames.LastOrDefault(), cts.Token);
            }

            _navigationStack = newStack;
        }
        catch (OperationCanceledException oex) when (timeoutToken is { IsCancellationRequested: true })
        {
            logger.LogWarning("Navigation timed out after {Timeout}", configuration.NavigationTimeout);
            throw new TimeoutException($"Navigation operation timed out after {configuration.NavigationTimeout}", oex);
        }
    }

    public async Task NavigateBackAsync(string outlet, CancellationToken cancel)
    {
        logger.LogInformation("Outlet {outlet} navigating back", outlet);
        using var cts = WithTimeout(cancel, out var timeoutToken);
        try
        {
            NavigationFrame[] outletFrames = [.. _navigationStack.Where(frame => frame.Outlet == outlet).TakeLast(2)];
            if (outletFrames.Length == 0)
            {
                logger.LogDebug("No navigation frames available for outlet {Outlet}, skipping navigation", outlet);
                return;
            }

            var lastFrameIndex = _navigationStack.IndexOf(outletFrames[^1]);
            var newFrame = outletFrames.Length == 2 ? outletFrames[0] : null;

            await UpdateOutletAsync(outlet, newFrame, cts.Token);

            _navigationStack.RemoveAt(lastFrameIndex);
        }
        catch (OperationCanceledException oex) when (timeoutToken is { IsCancellationRequested: true })
        {
            logger.LogWarning("Navigation timed out after {Timeout}", configuration.NavigationTimeout);
            throw new TimeoutException($"Navigation operation timed out after {configuration.NavigationTimeout}", oex);
        }
    }

    private async Task UpdateOutletAsync(string outlet, NavigationFrame? target, CancellationToken cancel)
    {
        logger.LogInformation("Updating navigation content of outlet {Outlet}", outlet);
        if(!_outlets.TryGetValue(outlet, out var outletData))
        {
            logger.LogDebug("Outlet {Outlet} not found, delaying navigation until that outlet is registered.", outlet);
            return;
        }

        await uiProxy.AwaitInitializationAsync(cancel);
        await uiProxy.RunUiAsync(async token =>
        {
            if (!await OnNavigatingAsync(outlet, outletData.CurrentFrame, target, token))
            {
                logger.LogInformation("Navigation for outlet {Outlet} has been cancelled", outlet);
                return;
            }
            
            LoadTarget(target);

            await InformNavigableAsync(outletData.CurrentFrame?.ViewModel, navigable => navigable.NavigatingFromAsync(token), nameof(INavigableViewModel.NavigatingFromAsync));
            await InformNavigableAsync(target?.ViewModel, navigable => navigable.NavigatingToAsync(token), nameof(INavigableViewModel.NavigatingToAsync));

            logger.LogDebug("Loading navigation content into outlet");
            outletData.Outlet.LoadContent(target?.View);
            _outlets[outlet] = new ValueTuple<INavigationOutlet, NavigationFrame?>(outletData.Outlet, target);

            await OnNavigatedAsync(outlet, outletData.CurrentFrame, target, token);
        }, cancel);
    }

    private async ValueTask InformNavigableAsync(object? viewModel, Func<INavigableViewModel, Task> func, string methodName)
    {
        if (viewModel is INavigableViewModel navigable)
        {
            logger.LogDebug("Calling {Method} for the view model that is being navigated to", methodName);
            await func(navigable);
        }
    }

    private void LoadTarget(NavigationFrame? frame)
    {
        if (frame is null)
        {
            return;
        }
        
        if (frame.View is null)
        {
            logger.LogInformation("Creating view '{ViewType}'", frame.Route.ViewType.Name);
            frame.View = serviceProvider.GetRequiredService(frame.Route.ViewType);
        }

        if (frame.ViewModel is null && frame.Route.ViewModelType is not null)
        {
            logger.LogInformation("Creating view model '{ViewModelType}'", frame.Route.ViewModelType.Name);
            frame.ViewModel = serviceProvider.GetRequiredService(frame.Route.ViewModelType);
            if (frame.View is FrameworkElement dataContextHolder)
            {
                dataContextHolder.DataContext = frame.ViewModel;
            }
        }
    }

    private async Task OnNavigatedAsync(string outlet, NavigationFrame? oldFrame, NavigationFrame? newFrame, CancellationToken cancel)
    {
        logger.LogDebug("Raising Navigated event");
        var args = new NavigationEventArgs(
            cancel,
            outlet,
            oldFrame,
            newFrame,
            false);
        Navigated?.Invoke(this, args);
        await args.AwaitDeferralsAsync(cancel);
    }

    private async Task<bool> OnNavigatingAsync(string outlet, NavigationFrame? oldFrame, NavigationFrame? newFrame, CancellationToken cancel)
    {
        logger.LogDebug("Raising Navigating event");
        var args = new NavigationEventArgs(
            cancel,
            outlet,
            oldFrame,
            newFrame,
            true);
        Navigated?.Invoke(this, args);
        await args.AwaitDeferralsAsync(cancel);
        return !args.IsCancelled;
    }

    private CancellationTokenSource WithTimeout(CancellationToken cancel, out CancellationToken? timeoutToken)
    {
        if (!configuration.NavigationTimeout.HasValue)
        {
            timeoutToken = null;
            return CancellationTokenSource.CreateLinkedTokenSource(cancel);
        }

        var ctsTimeout = new CancellationTokenSource(configuration.NavigationTimeout.Value);
        timeoutToken = ctsTimeout.Token;
        return CancellationTokenSource.CreateLinkedTokenSource(ctsTimeout.Token, cancel);
    }
}