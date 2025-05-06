using System;
using System.Threading;

namespace AlexBins.Utils.AsyncEvent;

/// <summary>
/// Defines an interface for managing the deferral of an asynchronous event, enabling the event to be signaled as complete or failed.
/// </summary>
public interface IEventDeferral
{
    /// <summary>
    /// Gets the <see cref="CancellationToken"/> associated with the current deferral.
    /// The event raiser may cancel the event at any time.
    /// This token reports this cancellation request.
    /// </summary>
    CancellationToken CancellationToken { get; }

    /// <summary>
    /// Marks the deferral as complete, signaling completion without exception.
    /// </summary>
    void Complete();

    /// <summary>
    /// Marks the deferral as failed, completing it with an exception.
    /// </summary>
    /// <param name="ex">The exception with which to complete the deferral.</param>
    void Fail(Exception ex);
}