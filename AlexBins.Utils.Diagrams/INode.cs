namespace AlexBins.Utils.Diagrams;

public interface INode
{
    string Name { get; set; }
    string? Description { get; set; }
    ICollection<INode> Children { get; }
}