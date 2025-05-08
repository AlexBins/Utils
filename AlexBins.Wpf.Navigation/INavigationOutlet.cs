namespace AlexBins.Wpf.Navigation;

public interface INavigationOutlet
{
    public const string Default = "default";
    void LoadContent(object? view);
}