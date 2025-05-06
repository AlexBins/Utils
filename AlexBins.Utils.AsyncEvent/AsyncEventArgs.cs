using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AlexBins.Utils.AsyncEvent;

/// <summary>
/// Asynchronous event.
/// </summary>
public class AsyncEventArgs(CancellationToken token) : EventArgs
{
    private readonly Lock _lock = new();
    private readonly TaskCompletionSource _tcs = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private readonly List<Exception> _exceptions = [];
    
    private int _deferralCount = 0;
    private bool _completeRequested = false;

    /// <summary>
    /// Obtains an event deferral that postpones the completion of an asynchronous event.
    /// </summary>
    /// <returns>An object representing the deferral for the event.</returns>
    /// <exception cref="InvalidOperationException">Thrown when a deferral is requested after the event completed.</exception>
    public IEventDeferral GetDeferral()
    {
        lock (_lock)
        {
            if (_completeRequested)
            {
                throw new InvalidOperationException("Cannot get a deferral after completion has been requested.");
            }

            _deferralCount++;
        }

        return new EventDeferral(OnDeferralCompletion, token);
    }

    private void OnDeferralCompletion(Exception? ex)
    {
        var shouldComplete = false;

        lock (_lock)
        {
            _deferralCount--;
            if (_deferralCount == 0 && _completeRequested)
            {
                shouldComplete = true;
            }
        }

        if (ex is not null)
        {
            _exceptions.Add(ex);
        }

        if (shouldComplete)
        {
            _tcs.TrySetResult();
        }
    }

    /// <summary>
    /// Waits for all deferrals to complete for an asynchronous event.
    /// </summary>
    /// <param name="cancel">A cancellation token that can be used to cancel the waiting for deferrals.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="AggregateException">Thrown if any of the event handlers encounter exceptions.</exception>
    public async Task AwaitDeferralsAsync(CancellationToken cancel)
    {
        lock (_lock)
        {
            _completeRequested = true;

            if (_deferralCount == 0)
            {
                _tcs.TrySetResult();
            }
        }

        await _tcs.Task.WaitAsync(cancel);
        if (_exceptions.Count > 0)
        {
            throw new AggregateException("Some event handlers encountered exceptions", _exceptions);
        }
    }

    /// <summary>
    /// Defers the event until the passed task completes.
    /// </summary>
    /// <param name="handler">A function that represents the asynchronous operation, taking a cancellation token as a parameter.</param>
    public void Defer(Func<CancellationToken, Task> handler)
    {
        var deferral = GetDeferral();
        _ = Task.Run(async () =>
        {
            try
            {
                await handler.Invoke(deferral.CancellationToken);
                deferral.Complete();
            }
            catch (Exception ex)
            {
                deferral.Fail(ex);
            }
        });
    }
}