using System.Windows.Threading;
using Microsoft.Extensions.Logging;

namespace AlexBins.Wpf.Navigation.Internal;

internal class UiProxy(
    ILogger<UiProxy> logger) 
    : IUiProxy
{
    private Dispatcher? _uiDispatcher;
    private TaskFactory? _uiTaskFactory;

    private readonly TaskCompletionSource<bool> _tcsInitialized = new();
    public bool IsInitialized => _tcsInitialized.Task is { IsCompletedSuccessfully: true, Result: true };

    private TaskFactory UiTaskFactory
        => _uiTaskFactory
           ?? throw new InvalidOperationException("UI access has not yet been configured");
    private Dispatcher UiDispatcher 
        => _uiDispatcher 
           ?? throw new InvalidOperationException("UI access has not yet been configured");

    public async ValueTask AwaitInitializationAsync(CancellationToken cancel)
    {
        if (IsInitialized)
        {
            return;
        }
        
        await _tcsInitialized.Task;
    }

    public async ValueTask InitializeAsync(Dispatcher dispatcher, CancellationToken cancel)
    {
        if (IsInitialized)
        {
            logger.LogWarning("Attempted to initialize UI access multiple times - All but the first attempt will be ignored");
            return;
        }
        
        _uiDispatcher = dispatcher;
        if (_uiDispatcher.CheckAccess())
        {
            _uiTaskFactory = new TaskFactory(TaskScheduler.FromCurrentSynchronizationContext());
            _tcsInitialized.TrySetResult(true);
            return;
        }

        var operation = dispatcher.InvokeAsync(() =>
        {
            _uiTaskFactory = new TaskFactory(TaskScheduler.FromCurrentSynchronizationContext());
        });
        await using var registration = cancel.Register(() => operation.Abort());
        await operation.Task;
        _tcsInitialized.TrySetResult(true);
    }

    public async Task RunUiAsync(Action action, CancellationToken cancel = default)
    {
        if (UiDispatcher.CheckAccess())
        {
            action();
            return;
        }
        await UiTaskFactory.StartNew(action, cancel);
    }

    public async Task<TResult> RunUiAsync<TResult>(Func<TResult> action, CancellationToken cancel = default)
    {
        if (UiDispatcher.CheckAccess())
        {
            return action();
        }
        return await UiTaskFactory.StartNew(action, cancel);
    }

    public async Task RunUiAsync(Func<CancellationToken, Task> action, CancellationToken cancel = default)
    {
        if (UiDispatcher.CheckAccess())
        {
            await action(cancel);
            return;
        }

        await UiTaskFactory.StartNew(async () => await action(cancel), cancel).Unwrap();
    }

    public async Task<TResult> RunUiAsync<TResult>(Func<CancellationToken, Task<TResult>> action, CancellationToken cancel = default)
    {
        if (UiDispatcher.CheckAccess())
        {
            return await action(cancel);
        }
        return await UiTaskFactory.StartNew(async () => await action(cancel), cancel).Unwrap();
    }
}