namespace AlexBins.Wpf.Navigation.Extensions;

public static class SemaphoreSlimExtensions
{
    private class SemaphoreContext(Action onDispose) : IDisposable
    {
        public void Dispose()
        {
            onDispose.Invoke();
        }
    }
    
    public static async Task<IDisposable> EnterAsync(this SemaphoreSlim semaphore, CancellationToken cancellationToken = default)
    {
        await semaphore.WaitAsync(cancellationToken);
        return new SemaphoreContext(() => semaphore.Release());
    }
}