namespace Iis.Interfaces.Ontology.Schema
{
    public enum SchemaSourceKind
    {
        File,
        Database,
        New
    }
    public interface IOntologySchemaSource
    {
        string Data { get; }
        SchemaSourceKind SourceKind { get; }
        string Title { get; }
    }
}