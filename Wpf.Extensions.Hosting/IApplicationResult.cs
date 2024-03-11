namespace Wpf.Extensions.Hosting;

/// <summary>
/// Represents the result of an application.
/// </summary>
public interface IApplicationResult
{
    /// <summary>
    /// Gets or sets the exit code of the application.
    /// </summary>
    int? ExitCode
    {
        get;
        set;
    }
}
