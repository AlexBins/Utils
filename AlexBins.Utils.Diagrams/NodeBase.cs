namespace AlexBins.Utils.Diagrams;

public abstract class NodeBase : INode
{
    public required  string Name { get; set; }
    public string? Description { get; set; } = null;
    public ICollection<INode> Children { get; init; } = [];
}