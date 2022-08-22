namespace Hosting.Wpf;

using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

internal sealed class WpfHostExecutor
{
    private readonly ILogger<WpfHostExecutor> _logger;

    public WpfHostExecutor(ILogger<WpfHostExecutor> logger)
    {
        _logger = logger;
    }

    internal int Run(IHost host, CancellationToken token)
    {

        var app = host.Services.GetService<WpfApplication>();
        if (app == null)
        {
            _logger.LogError("Host has not been configured for WPF");
            return -1;
        }

        _logger.LogInformation("Host starting");
        Task startup = host.StartAsync(token);

        try
        {
            startup.Wait(CancellationToken.None);
            _logger.LogInformation("Host startup completed");
        }
        catch (AggregateException ex)
        {
            ex.Handle(inner =>
            {
                _logger.LogError(inner, "Error on application startup");
                return true;
            });
            return -1;
        }

        using CancellationTokenRegistration registration = token.Register(() =>
        {
            app.Dispatcher.Invoke(() => app.Shutdown());
        });
        
        _logger.LogInformation("Running UI");
        int result = app.Run();
        if (token.IsCancellationRequested)
        {
            _logger.LogError("Ui execution cancelled due to an early cancellation request");
        }
        else
        {
            _logger.LogInformation("UI terminated");
        }

        _logger.LogInformation("Host shutting down");
        Task shutdown = host.StopAsync(token);
        try
        {
            shutdown.Wait(CancellationToken.None);
            _logger.LogInformation("Host shutdown completed");
        }
        catch (AggregateException ex)
        {
            ex.Handle(inner =>
            {
                _logger.LogError(inner, "Error on application shutdown");
                return true;
            });
            return -1;
        }

        return result;
    }
}