namespace Iis.Interfaces.Ontology.Schema
{
    public interface ISchemaCompareDiffItem
    {
        INodeTypeLinked NodeTypeFrom { get; set; }
        INodeTypeLinked NodeTypeTo { get; set; }
    }
}