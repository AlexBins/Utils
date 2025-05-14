namespace AlexBins.Utils.Diagrams.Uml;

public abstract class MemberNode : NodeBase, IMemberProvider
{
    public ICollection<IMember> Members { get; init; } = [];
}