namespace Iis.Interfaces.Ontology.Schema
{
    public interface ISchemaCompareDiffInfo
    {
        string NewValue { get; }
        string OldValue { get; }
        string PropertyName { get; }
    }
}