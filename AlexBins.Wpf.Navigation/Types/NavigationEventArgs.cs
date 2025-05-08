using AlexBins.Utils.AsyncEvent;
using AlexBins.Wpf.Navigation.Internal;

namespace AlexBins.Wpf.Navigation.Types;

public sealed class NavigationEventArgs : AsyncEventArgs
{
    public string Outlet { get; }
    public NavigationFrame? From { get; }
    public NavigationFrame? To { get; }
    public bool CanCancel { get; }
    public bool IsCancelled { get; private set; }
    
    
    internal NavigationEventArgs(
        CancellationToken cancel,
        string outlet,
        NavigationFrame? from,
        NavigationFrame? to,
        bool canCancel)
        : base(cancel)
    {
        Outlet = outlet;
        From = from;
        To = to;
        CanCancel = canCancel;
        IsCancelled = false;
    }

    public void Cancel()
    {
        if (!CanCancel)
        {
            throw new InvalidOperationException("Navigation cannot be cancelled.");
        }
        
        IsCancelled = true;
    }
}