using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using AlexBins.Wpf.Navigation;
using Microsoft.Extensions.Logging;

namespace TestWpfApp;

public partial class Page2 : UserControl
{
    public Page2()
    {
        InitializeComponent();
    }
}

public class Page2Vm(ILogger<Page2Vm> logger) : INotifyPropertyChanged, INavigableViewModel
{
    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
    
    private string _text = "Hello from Page2";
    
    public string Text
    {
        get => _text;
        set => SetField(ref _text, value);
    }

    public async Task NavigatingToAsync(CancellationToken cancel)
    {
        logger.LogInformation("Navigating to Page2");
    }

    public async Task NavigatingFromAsync(CancellationToken cancel)
    {
        logger.LogInformation("Navigating from Page2");
    }
}