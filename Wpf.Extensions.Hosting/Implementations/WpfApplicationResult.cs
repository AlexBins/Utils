namespace Wpf.Extensions.Hosting.Implementations;

internal class WpfApplicationResult : IApplicationResult
{
    private int? _exitCode;

    public int? ExitCode
    {
        get => _exitCode;
        set => _exitCode ??= value;
    }
}
