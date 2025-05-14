namespace AlexBins.Utils.Diagrams.Uml;

public interface IMember
{
    string Name { get; set; }
    IMemberType Type { get; set; }
    MemberKind Kind { get; set; }
    MemberVisibility Visibility { get; set; }
}