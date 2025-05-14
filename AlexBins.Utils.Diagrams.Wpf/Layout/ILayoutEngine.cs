using System.Windows;

namespace AlexBins.Utils.Diagrams.Wpf.Layout;

internal interface ILayoutEngine
{
    void Arrange(IDiagram diagram);
    Rect GetBounds(INode node);
    ConnectorInfo GetConnectorInfo(IConnector connector);
}

internal class DummyLayoutEngine : ILayoutEngine
{
    private Dictionary<INode, Rect> _nodeBounds = [];
    private Dictionary<IConnector, ConnectorInfo> _connectorInfos = [];
    
    public double Spacing { get; set; } = 10;
    public double NodeWidth { get; set; } = 50;
    public double NodeHeight { get; set; } = 30;
    
    public void Arrange(IDiagram diagram)
    {
        foreach (var node in diagram.Nodes)
        {
            Arrange(node);
        }

        foreach (var connector in diagram.Connectors)
        {
            Arrange(connector);
        }
    }

    private void Arrange(INode node)
    {
        if (_nodeBounds.ContainsKey(node))
        {
            throw new InvalidOperationException("Cannot have the same node added multiple times.");
        }
    }

    private void Arrange(IConnector connector)
    {
        
    }

    public Rect GetBounds(INode node)
    {
        throw new NotImplementedException();
    }

    public ConnectorInfo GetConnectorInfo(IConnector connector)
    {
        throw new NotImplementedException();
    }
}