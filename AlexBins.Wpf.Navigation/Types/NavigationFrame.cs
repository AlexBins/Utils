using System.Text;

namespace AlexBins.Wpf.Navigation.Types;

public class NavigationFrame
{
    internal NavigationFrame(Route route,
        Dictionary<string, object> parameters)
    {
        Route = route;
        Outlet = route.Outlet;
        Fragment = route.Name;
        Parameters = parameters;
    }

    public Route Route { get; }
    public string Outlet { get; }
    public string Fragment { get; }
    public Dictionary<string, object> Parameters { get; }
    public object? View { get; set; }
    public object? ViewModel { get; set; }

    public string ToRouteFragment()
    {
        var sb = new StringBuilder();
        sb.Append(Outlet);
        sb.Append(':');
        sb.Append(Fragment);
        if (Parameters.Count > 0)
        {
            sb.Append('?');
            sb.AppendJoin('&', Parameters.Select(kvp => $"{kvp.Key}={kvp.Value}"));
        }
        return sb.ToString();
    }
}