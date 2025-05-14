namespace AlexBins.Utils.Diagrams.Uml.Connectors;

public class UmlAssociation : ConnectorBase
{
    public int SourceMultiplicity { get; set; } = 1;
    public int TargetMultiplicity { get; set; } = 1;
}