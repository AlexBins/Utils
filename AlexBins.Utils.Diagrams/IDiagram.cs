namespace AlexBins.Utils.Diagrams;

public interface IDiagram
{
    ICollection<INode> Nodes { get; }
    ICollection<IConnector> Connectors { get; }
}