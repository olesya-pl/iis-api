using Iis.Interfaces.Ontology.Schema;

namespace Iis.OntologySchema.DataTypes
{
    public class SchemaRelationType: SchemaRelationTypeRaw, IRelationType, IRelationTypeLinked
    {
        public INodeTypeLinked NodeType { get; internal set; }
        public INodeTypeLinked SourceType { get; internal set; }
        public INodeTypeLinked TargetType { get; internal set; }
        public override string ToString()
        {
            return $"{SourceType.Name}.{NodeType.Name}";
        }
        public bool IsIdentical(IRelationTypeLinked relationType, bool includeTargetType)
        {
            return Kind == relationType.Kind
                && EmbeddingOptions == relationType.EmbeddingOptions
                && SourceType.Name == relationType.SourceType.Name
                && TargetType.Name == relationType.TargetType.Name
                && (!includeTargetType || TargetType.IsIdentical(relationType.TargetType));
        }
    }
}
