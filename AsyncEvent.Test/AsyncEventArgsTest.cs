using AlexBins.Utils.AsyncEvent;

namespace AsyncEvent.Test;

[CancelAfter(2_000)]
[TestFixture]
public class AsyncEventArgsTest
{
    private AsyncEventProvider _events = null!;

    private class AsyncEventProvider
    {
        public event EventHandler<AsyncEventArgs>? AsyncEvent;
        public async Task OnEvent(CancellationToken cancel)
        {
            var args = new AsyncEventArgs(cancel);
            AsyncEvent?.Invoke(this, args);
            await args.AwaitDeferralsAsync(cancel);
        }
    }
    
    [SetUp]
    public void Setup()
    {
        _events = new();
    }

    [Test]
    public void ShouldCompleteImmediatelyWithNoDeferral(CancellationToken cancel)
    {
        Assert.DoesNotThrowAsync(async () => await _events.OnEvent(cancel));
    }

    [Test]
    public async Task ShouldCompleteOnlyWhenDeferralIsCompleted(CancellationToken cancel)
    {
        const int handlerToken = 0;
        const int raiserToken = 1;

        TaskCompletionSource<int> tcs = new();
        
        AddAsyncHandler(async () =>
        {
            await Task.Yield();
            tcs.TrySetResult(handlerToken);
        });
        await _events.OnEvent(cancel);
        tcs.TrySetResult(raiserToken);
        
        Assert.That(await tcs.Task, Is.EqualTo(handlerToken));
    }

    [Test]
    public void ReportsHandlerError(CancellationToken cancel)
    {
        AddAsyncHandler(() => throw new Exception("Test 1"));
        AddAsyncHandler(() => throw new Exception("Test 2"));
        
        var encountered = Assert.ThrowsAsync<AggregateException>(async () => await _events.OnEvent(cancel));
        Assert.That(encountered.InnerExceptions, Has.Count.EqualTo(2));
    }

    [Test]
    public async Task IgnoresImmediatelyCompletedDeferrals(CancellationToken cancel)
    {
        _events.AsyncEvent += (_, e) =>
        {
            var deferral = e.GetDeferral();
            deferral.Complete();
        };
        TaskCompletionSource<bool> handlerTcs = new();
        
        AddAsyncHandler(async () =>
        {
            await Task.Yield();
            handlerTcs.TrySetResult(true);
        });

        await _events.OnEvent(cancel);
        handlerTcs.TrySetResult(false);
        Assert.That(await handlerTcs.Task, Is.True);
    }

    private void AddAsyncHandler(Func<Task> handler)
    {
        AddAsyncHandler(async _ => await handler());
    }

    private void AddAsyncHandler(Func<CancellationToken, Task> handler)
    {
        _events.AsyncEvent += (_, e) => e.Defer(handler);
    }
}