namespace Bayoomed.NeoDoppler.Utils.AsyncEvent
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    /// <summary>
    /// Asynchronous event.
    /// </summary>
    /// <typeparam name="TArg">Event argument type</typeparam>
    public struct AsyncEvent<TArg>
    {
        /// <summary>
        /// The event handler delegate definition
        /// </summary>
        /// <param name="sender">The instance that raised the event</param>
        /// <param name="arg">The event argument</param>
        /// <returns>Awaitable task completing when the event is handled</returns>
        public delegate Task Handler(object? sender, TArg arg);

        private IList<Handler> _handlers;

        /// <summary>
        /// Adds a handler to an event
        /// </summary>
        /// <param name="event">The event to add to</param>
        /// <param name="handler">The handler to add</param>
        /// <returns>The event including the handler</returns>
        public static AsyncEvent<TArg> operator +(AsyncEvent<TArg> @event, Handler handler)
        {
            @event._handlers ??= new List<Handler>();
            @event._handlers.Add(handler);
            return @event;
        }
        /// <summary>
        /// Removes a handler from an event if it is registered
        /// </summary>
        /// <param name="event">The event to remove from</param>
        /// <param name="handler">The handler to remove</param>
        /// <returns>The event without the handler</returns>
        public static AsyncEvent<TArg> operator -(AsyncEvent<TArg> @event, Handler handler)
        {
            @event._handlers?.Remove(handler);
            return @event;
        }

        /// <summary>
        /// Invokes the event asynchronously
        /// </summary>
        /// <param name="sender">The instance that raised the event</param>
        /// <param name="arg">The event argument</param>
        /// <returns>Task completing when each event handler completed</returns>
        public async ValueTask InvokeAsync(object? sender, TArg arg)
        {
            if (_handlers == null) return;

            await Task.WhenAll(_handlers.Select(h => h.Invoke(sender, arg)));
        }
    }
}
