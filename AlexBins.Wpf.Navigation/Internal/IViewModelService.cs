using System.Windows;
using AlexBins.Wpf.Navigation.Types;

namespace AlexBins.Wpf.Navigation.Internal;

internal interface IViewModelService
{
    bool TrySetViewModel(object? container, object? dataContext);
}

internal class FrameworkElementViewModelService(
    NavigationServiceConfiguration config)
    : IViewModelService
{
    public bool TrySetViewModel(object? container, object? dataContext)
    {
        if (container is FrameworkElement element &&
            (config.OverrideExistingViewModels || element.DataContext is null))
        {
            element.DataContext = dataContext;
            return true;
        }

        return false;
    }
}