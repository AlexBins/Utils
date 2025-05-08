using System.Windows.Threading;

namespace AlexBins.Wpf.Navigation;

public interface IUiProxy
{
    bool IsInitialized { get; }
    ValueTask AwaitInitializationAsync(CancellationToken cancel);
    ValueTask InitializeAsync(Dispatcher dispatcher, CancellationToken cancel);
    Task RunUiAsync(Action action, CancellationToken cancel = default);
    Task<TResult> RunUiAsync<TResult>(Func<TResult> action, CancellationToken cancel = default);
    Task RunUiAsync(Func<CancellationToken, Task> action, CancellationToken cancel = default);
    Task<TResult> RunUiAsync<TResult>(Func<CancellationToken, Task<TResult>> action, CancellationToken cancel = default);
}