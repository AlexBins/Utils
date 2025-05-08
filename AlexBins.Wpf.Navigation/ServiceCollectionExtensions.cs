using AlexBins.Wpf.Navigation.Internal;
using AlexBins.Wpf.Navigation.Types;
using Microsoft.Extensions.DependencyInjection;

namespace AlexBins.Wpf.Navigation;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddNavigationServices(this IServiceCollection services,
        NavigationServiceConfiguration? config = null)
        => services.AddNavigationServices(_ => config ?? new NavigationServiceConfiguration());
    public static IServiceCollection AddNavigationServices(this IServiceCollection services, Func<IServiceProvider, NavigationServiceConfiguration> configFactory)
    {
        return services
            .AddSingleton<INavigationService, NavigationService>()
            .AddSingleton<IUiProxy, UiProxy>()
            .AddSingleton(configFactory.Invoke)
            .AddHostedService<NavigationBackgroundService>()
            .AddSingleton<IViewModelService, FrameworkElementViewModelService>();
    }

    public static IServiceCollection AddRoute<TRoute>(
        this IServiceCollection services,
        TRoute route,
        Func<IServiceProvider, object>? viewFactory = null,
        Func<IServiceProvider, object>? viewModelFactory = null)
        where TRoute : Route
    {
        services.AddSingleton<Route>(route);
        
        services.Add(viewFactory is null
            ? ServiceDescriptor.Transient(route.ViewType, route.ViewType)
            : ServiceDescriptor.Transient(route.ViewType, viewFactory));

        if (route.ViewModelType is not null)
        {
            services.Add(viewModelFactory is null
                ? ServiceDescriptor.Transient(route.ViewModelType, route.ViewModelType)
                : ServiceDescriptor.Transient(route.ViewModelType, viewModelFactory));
        }

        return services;
    }
}