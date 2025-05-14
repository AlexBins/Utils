using System.ComponentModel;

namespace AlexBins.Utils.Diagrams.Wpf;

internal class DiagramController : INotifyPropertyChanged, IDiagramController
{
    public IDiagram? Diagram { get; private set; }

    public void UpdateDiagram(IDiagram newDiagram)
    {
        Diagram = newDiagram;
        NotifyDiagramUpdated();
    }

    public void NotifyDiagramUpdated()
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Diagram)));
    }
    
    public event PropertyChangedEventHandler? PropertyChanged;
}