namespace AlexBins.Utils.Diagrams.Wpf;

public interface IDiagramController
{
    IDiagram? Diagram { get; }
    void UpdateDiagram(IDiagram newDiagram);
    void NotifyDiagramUpdated();
}