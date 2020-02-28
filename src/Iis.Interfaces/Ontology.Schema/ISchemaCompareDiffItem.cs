namespace Iis.Interfaces.Ontology.Schema
{
    public interface ISchemaCompareDiffItem
    {
        INodeTypeLinked NewNode { get; set; }
        INodeTypeLinked OldNode { get; set; }
    }
}