namespace AlexBins.Utils.AsyncEvent;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Asynchronous event.
/// </summary>
/// <typeparam name="TArg">Event argument type</typeparam>
public class AsyncEventArgs<TArg> : EventArgs
{
    private readonly record struct ProcessorDefinition(Func<CancellationToken, Task> Processor, CancellationToken CancellationToken);

    private readonly List<ProcessorDefinition> _processorDefinitions = [];

    /// <summary>
    /// Adds a processor to the event.
    /// </summary>
    /// <param name="processor">The processor function to be added.</param>
    /// <param name="cancellationToken">The cancellation token for the processor.</param>
    public void AddProcessor(Func<CancellationToken, Task> processor, CancellationToken cancellationToken = default)
    {
        _processorDefinitions.Add(new ProcessorDefinition(processor, cancellationToken));
    }

    /// <summary>
    /// Awaits all the processors added to the event.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token for the await operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task AwaitProcessorsAsync(CancellationToken cancellationToken = default)
    {
        await Task.WhenAll(
            _processorDefinitions
            .Select(
                p =>
                {
                    return p.CancellationToken.IsCancellationRequested
                        ? Task.FromCanceled(p.CancellationToken)
                        : p.Processor(
                            CancellationTokenSource.CreateLinkedTokenSource(
                                cancellationToken,
                                p.CancellationToken)
                        .Token);
                }));
    }
}
