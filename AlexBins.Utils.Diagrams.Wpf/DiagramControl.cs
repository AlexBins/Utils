using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace AlexBins.Utils.Diagrams.Wpf;

public class DiagramControl : Control
{
    static DiagramControl()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(DiagramControl),
            new FrameworkPropertyMetadata(typeof(DiagramControl)));
    }

    public static readonly DependencyProperty DiagramControllerProperty = DependencyProperty.Register(
        nameof(DiagramController), 
        typeof(IDiagramController),
        typeof(DiagramControl),
        new UIPropertyMetadata(
            null,
            DiagramControllerChanged));

    private static void DiagramControllerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not DiagramControl control)
        {
            return;
        }
        
        if (e.OldValue is INotifyPropertyChanged oldController)
        {
            oldController.PropertyChanged -= control.HandleDiagramControllerPropertyChanged;
        }

        if (e.NewValue is INotifyPropertyChanged newController)
        {
            newController.PropertyChanged += control.HandleDiagramControllerPropertyChanged;
        }
    }

    public IDiagramController DiagramController
    {
        get => (IDiagramController)GetValue(DiagramControllerProperty);
        set => SetValue(DiagramControllerProperty, value);
    }

    private void HandleDiagramControllerPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(IDiagramController.Diagram))
        {
            return;
        }
        
        
    }
}