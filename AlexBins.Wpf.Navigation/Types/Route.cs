namespace AlexBins.Wpf.Navigation.Types;

public class Route(
    string name,
    Type viewType,
    Type? viewModelType = null,
    string outlet = INavigationOutlet.Default,
    bool isHome = false)
{
    public string Name { get; } = name;
    public Type ViewType { get; } = viewType;
    public Type? ViewModelType { get; } = viewModelType;
    public string Outlet { get; } = outlet;
    public bool IsHome { get; } = isHome;
}

public class Route<TView, TViewModel>(string name, string outlet = INavigationOutlet.Default, bool isHome = false)
    :Route(name, typeof(TView), typeof(TViewModel), outlet, isHome)
    where TView: class
    where TViewModel: class
{
}

public class Route<TView>(string name, string outlet = INavigationOutlet.Default, bool isHome = false)
    : Route(name, typeof(TView), null, outlet, isHome)
    where TView: class
{
}