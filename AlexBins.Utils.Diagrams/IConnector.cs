namespace AlexBins.Utils.Diagrams;

public interface IConnector
{
    INode Source { get; set; }
    INode Target { get; set; }
}

public abstract class ConnectorBase : IConnector
{
    public required INode Source { get; set; }
    public required INode Target { get; set; }
}