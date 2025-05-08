namespace AlexBins.Wpf.Navigation;

public interface INavigableViewModel
{
    Task NavigatingToAsync(CancellationToken cancel);
    Task NavigatingFromAsync(CancellationToken cancel);
}