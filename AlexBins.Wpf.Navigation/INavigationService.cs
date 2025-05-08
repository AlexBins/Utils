using System.Windows.Threading;
using AlexBins.Wpf.Navigation.Types;

namespace AlexBins.Wpf.Navigation;

public interface INavigationService
{
    IReadOnlyCollection<Route> RouteConfiguration { get; }
    string CurrentRoute { get; }
    
    event EventHandler<NavigationEventArgs> Navigated;
    event EventHandler<NavigationEventArgs> Navigating;
    
    Task RegisterOutletAsync(string name, INavigationOutlet outlet, CancellationToken cancel);
    
    Task NavigateAsync(string path, Dictionary<string, object>? parameters, CancellationToken cancel);
    Task NavigateAsync(string path, CancellationToken cancel) => NavigateAsync(path, null, cancel);
    Task NavigateBackAsync(CancellationToken cancel) => NavigateAsync("..", null, cancel);
    Task NavigateBackAsync(string outlet, CancellationToken cancellationToken = default);
}