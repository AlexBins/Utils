namespace Wpf.Extensions.Hosting;

/// <summary>
/// Provides access to the UI thread.
/// </summary>
public interface IUiAccess
{
    /// <summary>
    /// Runs the specified action on the UI thread asynchronously.
    /// </summary>
    /// <param name="action">The action to run.</param>
    /// <param name="cancel">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task RunUiAsync(Action action, CancellationToken cancel = default);

    /// <summary>
    /// Runs the specified function on the UI thread asynchronously.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="func">The function to run.</param>
    /// <param name="cancel">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation and containing the result of the function.</returns>
    Task<TResult> RunUiAsync<TResult>(Func<TResult> func, CancellationToken cancel = default);
}
