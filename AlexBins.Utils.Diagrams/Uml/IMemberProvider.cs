namespace AlexBins.Utils.Diagrams.Uml;

public interface IMemberProvider
{
    ICollection<IMember> Members { get; }
}