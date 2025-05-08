using System.Collections.Immutable;
using System.Text.RegularExpressions;
using AlexBins.Wpf.Navigation.Types;

namespace AlexBins.Wpf.Navigation.Internal;

internal static partial class RouteAnalyzer
{
    public record RouteAnalyzationResult(
        List<NavigationFrame> NewStack,
        List<NavigationFrame> DroppedFrames);
    public static RouteAnalyzationResult AppendRoute(
        IReadOnlyCollection<NavigationFrame> navigationStack,
        IReadOnlyCollection<Route> routeConfiguration,
        string path,
        Dictionary<string, object> parameters)
    {
        List<NavigationFrame> droppedFrames = [];
        var isAbsolute = path.StartsWith('/');
        var newStack = isAbsolute ? [] : new List<NavigationFrame>(navigationStack);
        
        var fragments = new List<string>(path.Split('/', StringSplitOptions.RemoveEmptyEntries));
        // Remove most '..' and all '.' fragments
        SimplifyRoute(fragments);
        
        // The only '..' fragments left are at the very beginning => Remove those from the existing stack
        var backCount = fragments.TakeWhile(fragment => fragment.StartsWith("..")).Count();
        if (backCount > newStack.Count)
        {
            throw new InvalidOperationException("Cannot navigate back beyond the root");
        }

        droppedFrames.AddRange(newStack[^backCount..]);
        newStack.RemoveRange(newStack.Count - backCount, backCount);

        fragments.RemoveRange(0, backCount);
        
        // Create frames for each remaining fragment and add them to the stack
        newStack.AddRange(fragments.Select(fragment => ParseFragment(routeConfiguration, fragment)));

        // Add any potential parameters
        if (newStack.Count <= 0)
        {
            throw new InvalidOperationException("Navigation led to an empty navigation stack");
        }

        foreach (var (key, value) in parameters)
        {
            newStack[^1].Parameters[key] = value;
        }

        return new RouteAnalyzationResult(newStack, droppedFrames);
    }
    
    private static NavigationFrame ParseFragment(IReadOnlyCollection<Route> routeConfiguration, string fragment)
    {
        var pattern = FragmentRegex();
        var match = pattern.Match(fragment);
        if (!match.Success)
        {
            throw new InvalidOperationException($"Invalid fragment '{fragment}'");
        }

        var route = match.Groups["fragment"].Value;
        var query = match.Groups["query"].Success ? match.Groups["query"].Value : string.Empty;
        var routeMatch = routeConfiguration.FirstOrDefault(r => r.Name == route);
        if (routeMatch is null)
        {
            throw new InvalidOperationException($"Route '{route}' not found in configuration");
        }

        var queryPattern = QueryRegex();
        Dictionary<string, object> parameters = [];
        foreach (Match queryMatch in queryPattern.Matches(query))
        {
            var para = queryMatch.Groups["para"].Value;
            var value = queryMatch.Groups["value"].Value;
            parameters[para] = value;
        }
        
        return new NavigationFrame(routeMatch, parameters);
    }

    private static void SimplifyRoute(List<string> fragments)
    {
        // Sanitize 'back' and 'current' steps
        for (var i = 0; i < fragments.Count; i++)
        {
            if (fragments[i].StartsWith(".."))
            {
                if (fragments.Count > 1)
                {
                    fragments.RemoveRange(i - 1, 2);
                    i -= 2;
                }
            }
            else if (fragments[i].StartsWith('.'))
            {
                fragments.RemoveAt(i);
                i--;
            }
        }
    }

    [GeneratedRegex(@"^(?<fragment>[a-zA-Z0-9\-\.]+)(\?(?<query>[^\/]*))?$")]
    private static partial Regex FragmentRegex();

    [GeneratedRegex(@"(?<para>[a-zA-Z0-9\-\.]+)=(?<value>[a-zA-Z0-9\-\.]+)")]
    private static partial Regex QueryRegex();
}