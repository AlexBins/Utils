using System;
using System.Threading;
using CommunityToolkit.Diagnostics;

namespace AlexBins.Utils.AsyncEvent;

/// <summary>
/// Represents a deferral for an asynchronous event, allowing the event to be deferred until the deferral is completed.
/// </summary>
internal sealed class EventDeferral : IEventDeferral
{
    private readonly Action<Exception?> _onComplete;
    private bool _isCompleted;

    /// <summary>
    /// Gets the <see cref="CancellationToken"/> associated with the current deferral.
    /// The event raiser may cancel the event at any time.
    /// This token reports this cancellation request.
    /// </summary>
    public CancellationToken CancellationToken { get; }

    internal EventDeferral(
        Action<Exception?> onComplete,
        CancellationToken cancellationToken)
    {
        Guard.IsNotNull(onComplete);
        
        _onComplete = onComplete;
        CancellationToken = cancellationToken;
    }

    private void Complete(Exception? ex)
    {
        if (_isCompleted)
        {
            return;
        }

        _onComplete.Invoke(ex);
        _isCompleted = true;
    }

    /// <summary>
    /// Marks the deferral as failed, completing it with an exception.
    /// </summary>
    /// <param name="ex">The exception with which to complete the deferral.</param>
    public void Fail(Exception ex) => Complete(ex);

    /// <summary>
    /// Marks the deferral as complete, signaling completion without exception.
    /// </summary>
    public void Complete() => Complete(null);
}