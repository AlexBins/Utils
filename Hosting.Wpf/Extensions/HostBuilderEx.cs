namespace Hosting.Wpf.Extensions;

using System;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

/// <summary>
/// Provides extensions for <see cref="IHostBuilder"/> to integrate WPF functionality
/// </summary>
public static class HostBuilderEx
{
    /// <summary>
    /// Configures the host builder to support WPF functionality
    /// </summary>
    /// <param name="hostBuilder">The host builder to configure</param>
    /// <param name="uiConfiguration">Configuration method to add UI components to the service collection</param>
    /// <returns>The configured host builder</returns>
    public static IHostBuilder ConfigureWpf(
        this IHostBuilder hostBuilder,
        Action<IServiceCollection>? uiConfiguration = default)
    {
        return hostBuilder.ConfigureServices(
                svc =>
                {
                    svc.AddSingleton<WpfApplication>();
                    svc.AddSingleton<PageHost>();
                    svc.AddSingleton<WpfHostExecutor>();
                })
            .ConfigureServices(uiConfiguration ?? new Action<IServiceCollection>(_ => { }));
    }

    /// <summary>
    /// Runs the WPF application. This can only be done synchronously and must be called from an STA thread.
    /// </summary>
    /// <param name="hostBuilder">The configured host builder containing all services for the application</param>
    /// <param name="token">Cancellation token if early cancellation is requested</param>
    /// <returns>Execution result code</returns>
    public static int RunWpf(this IHostBuilder hostBuilder, CancellationToken token = default)
    {
        using IHost host = hostBuilder.Build();

        var runner = host.Services.GetRequiredService<WpfHostExecutor>();
        return runner.Run(host, token);
    }
}