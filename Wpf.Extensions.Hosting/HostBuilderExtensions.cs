using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Wpf.Extensions.Hosting.Implementations;

namespace Wpf.Extensions.Hosting;

/// <summary>
/// Provides extension methods for configuring the <see cref="IHostBuilder"/> for WPF applications.
/// </summary>
public static class HostBuilderExtensions
{
    internal const string WindowServiceKey = "App-Windows";

    /// <summary>
    /// Configures the <see cref="IHostBuilder"/> for a WPF application.
    /// </summary>
    /// <typeparam name="TApplication">The type of the WPF application.</typeparam>
    /// <param name="hostBuilder">The <see cref="IHostBuilder"/> to configure.</param>
    /// <param name="configureDelegate">An optional delegate to configure the <see cref="IServiceCollection"/>.</param>
    /// <returns>The configured <see cref="IHostBuilder"/>.</returns>
    public static IHostBuilder ConfigureWpf<TApplication>(
        this IHostBuilder hostBuilder,
        Action<HostBuilderContext, IServiceCollection>? configureDelegate = null)
        where TApplication : Application, new()
    {
        configureDelegate ??= (_, _) => { };

        return hostBuilder
            .ConfigureServices((ctx, svc) =>
            {
                svc.AddHostedService<WpfService<TApplication>>();
                svc.AddSingleton(provider => provider.GetRequiredService<WpfService<TApplication>>().Application);
                svc.AddSingleton(provider => provider.GetRequiredService<WpfService<TApplication>>().UiAccess);
                svc.AddSingleton<IApplicationResult, WpfApplicationResult>();

                var buffer = new ServiceCollection();
                configureDelegate(ctx, buffer);
                foreach (var service in buffer)
                {
                    AddServiceDescriptor(svc, service);
                }
            });
    }

    private static void AddServiceDescriptor(IServiceCollection svc, ServiceDescriptor service)
    {
        svc.Add(service);

        if (service.ImplementationType is not null)
        {
            svc.Add(
                new ServiceDescriptor(
                    service.ServiceType,
                    WindowServiceKey,
                    service.ImplementationType,
                    service.Lifetime));
        }
        if (service.ImplementationInstance is not null)
        {
            svc.Add(
                new ServiceDescriptor(
                    service.ServiceType,
                    WindowServiceKey,
                    service.ImplementationInstance));
        }
        if (service.ImplementationFactory is not null)
        {
            svc.Add(
                new ServiceDescriptor(
                    service.ServiceType,
                    WindowServiceKey,
                    (provider, _) => service.ImplementationFactory(provider),
                    service.Lifetime));
        }
    }
}
