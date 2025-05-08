using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace AlexBins.Wpf.Navigation.Extensions;

public static class NavigationServiceExtensions
{
    private sealed class NavigationOutletProxy : INavigationOutlet
    {
        private readonly ILogger<NavigationOutletProxy> _logger;
        private readonly Action<object?> _loadAction;

        public NavigationOutletProxy(ILogger<NavigationOutletProxy> logger,
            Action<object?> loadAction)
        {
            _logger = logger;
            _loadAction = loadAction;
            
            _logger.LogInformation("Created a new navigation outlet proxy");
        }

        public void LoadContent(object? view)
        {
            _logger.LogInformation("Navigation outlet proxy is loading a new view into the passed delegate");
            _loadAction.Invoke(view);
        }
    }

    private static ILoggerFactory? _loggerFactory;

    private static ILoggerFactory LoggerFactory
    {
        get
        {
            try
            {
                _loggerFactory ??= Ioc.Default.GetRequiredService<ILoggerFactory>();
            }
            catch
            {
                _loggerFactory ??= NullLoggerFactory.Instance;
            }

            return _loggerFactory;
        }
    }
    
    public static Task RegisterOutletAsync(this INavigationService navigationService, string name, Action<object?> loadAction, CancellationToken cancel)
    {
        return navigationService.RegisterOutletAsync(name, new NavigationOutletProxy(LoggerFactory.CreateLogger<NavigationOutletProxy>(), loadAction), cancel);
    }
}