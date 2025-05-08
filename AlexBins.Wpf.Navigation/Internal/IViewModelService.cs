using System.Windows;

namespace AlexBins.Wpf.Navigation.Internal;

internal interface IViewModelService
{
    bool TrySetViewModel(object? container, object? dataContext);
}

internal class FrameworkElementViewModelService : IViewModelService
{
    public bool TrySetViewModel(object? container, object? dataContext)
    {
        if (container is FrameworkElement element)
        {
            element.DataContext = dataContext;
            return true;
        }

        return false;
    }
}