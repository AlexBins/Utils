namespace AlexBins.Wpf.Navigation.Types;

public record NavigationServiceConfiguration
{
    public TimeSpan? NavigationTimeout { get; init; } = TimeSpan.FromSeconds(2);
    public bool OverrideExistingViewModels { get; init; } = false;
}