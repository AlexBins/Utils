using System.Windows.Threading;

namespace Wpf.Extensions.Hosting.Implementations;

internal class WpfUiAccess : IUiAccess
{
    private Dispatcher _dispatcher;

    public WpfUiAccess(Dispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    public Task RunUiAsync(Action action, CancellationToken cancel = default)
    {
        return _dispatcher.InvokeAsync(action, DispatcherPriority.Normal, cancel).Task;
    }

    public Task<TResult> RunUiAsync<TResult>(Func<TResult> func, CancellationToken cancel = default)
    {
        return _dispatcher.InvokeAsync(func, DispatcherPriority.Normal, cancel).Task;
    }
}
