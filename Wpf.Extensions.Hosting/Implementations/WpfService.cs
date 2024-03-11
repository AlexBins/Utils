using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Wpf.Extensions.Hosting.Implementations;

internal class WpfService<TApplication>(
    ILogger<WpfService<TApplication>> logger,
    IServiceProvider services,
    IHostApplicationLifetime lifetime,
    IApplicationResult applicationResult)
    : IHostedService
    where TApplication : Application, new()
{
    private readonly ManualResetEventSlim _mreUiCreated = new(false);
    private readonly TaskCompletionSource<int> _applicationResultCompletionSource = new();

    private TApplication? _application;
    private IUiAccess? _uiAccess;
    private Thread? _uiThread;
    private bool _isInitialized = false;

    public Application Application
    {
        get
        {
            if (!_isInitialized)
            {
                _mreUiCreated.Wait(lifetime.ApplicationStopping);
            }

            return _application!;
        }
    }

    public IUiAccess UiAccess
    {
        get
        {
            if (!_isInitialized)
            {
                _mreUiCreated.Wait(lifetime.ApplicationStopping);
            }

            return _uiAccess!;
        }
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
#if !WINDOWS
        throw new NotSupportedException("This service is only supported on Windows");
#endif

        await StartUiThreadAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        ReactToHostShutdown();

        using var stopRegistration = cancellationToken.Register(() => _applicationResultCompletionSource.TrySetCanceled(cancellationToken));

        var exitCode = await _applicationResultCompletionSource.Task.ConfigureAwait(false);
        applicationResult.ExitCode = exitCode;
    }

    public async Task StartUiThreadAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Preparing for UI thread startup");

        TaskCompletionSource taskCompletionSource = new();
        using var registration = cancellationToken.Register(() =>
        {
            try
            {
                _uiThread?.Interrupt();
            }
            catch
            {
                // Ignore
            }

            taskCompletionSource.TrySetCanceled(cancellationToken);
        });

        _uiThread = new Thread(RunUi);
#pragma warning disable CA1416 // Validate platform compatibility
        _uiThread.SetApartmentState(ApartmentState.STA);
#pragma warning restore CA1416 // Validate platform compatibility
        _uiThread.Start(taskCompletionSource);

        // No async possible here
        await taskCompletionSource.Task.ConfigureAwait(false);
        logger.LogInformation("UI thread started successfully");
    }

    private void RunUi(object? state)
    {
        var startupCompletion = (state as TaskCompletionSource)!;

        logger.LogDebug("Creating application and UI access instances");
        _application = new TApplication();
        _uiAccess = new WpfUiAccess(_application.Dispatcher);
        _isInitialized = true;

        logger.LogDebug("Unblocking access to application and UI access instances");
        _mreUiCreated.Set();

        logger.LogDebug("Reporting UI thread startup completion to the main thread");
        startupCompletion.TrySetResult();

        // React to host shutdown
        using var stoppingRegistration = lifetime.ApplicationStopping.Register(ReactToHostShutdown);

        _application.Startup += HandleApplicationStartup;

        logger.LogInformation("Starting UI");
        _applicationResultCompletionSource.TrySetResult(_application.Run());
        logger.LogInformation("UI terminated");

        if (!lifetime.ApplicationStopping.IsCancellationRequested)
        {
            logger.LogDebug("Stopping application");
            lifetime.StopApplication();
        }
    }

    private void ReactToHostShutdown()
    {
        if (_application?.Dispatcher.HasShutdownStarted == false && !_application.Dispatcher.HasShutdownFinished)
        {
            logger.LogInformation("Shutting down UI due to the application shutting down");
            _application.Dispatcher.InvokeShutdown();
        }
    }

    private void HandleApplicationStartup(object sender, StartupEventArgs e)
    {
        logger.LogDebug("Showing all windows registered");
        var windowServices = services.GetKeyedServices<Window>(HostBuilderExtensions.WindowServiceKey);
        foreach (var window in windowServices)
        {
            window.Show();
        }
    }
}