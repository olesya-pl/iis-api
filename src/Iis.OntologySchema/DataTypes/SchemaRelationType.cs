using Iis.Interfaces.Ontology.Schema;
using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.OntologySchema.DataTypes
{
    public class SchemaRelationType: IRelationType, IRelationTypeLinked
    {
        public Guid Id { get; set; }
        public RelationKind Kind { get; set; }
        public EmbeddingOptions EmbeddingOptions { get; set; }
        public Guid SourceTypeId { get; set; }
        public Guid TargetTypeId { get; set; }
        public INodeTypeLinked NodeType { get; set; }
        public INodeTypeLinked SourceType { get; set; }
        public INodeTypeLinked TargetType { get; set; }
        public override string ToString()
        {
            return $"{SourceType.Name}.{NodeType.Name}";
        }
        public bool IsEqual(IRelationType relationType)
        {
            return Id == relationType.Id
                && Kind == relationType.Kind
                && EmbeddingOptions == relationType.EmbeddingOptions
                && SourceTypeId == relationType.SourceTypeId
                && TargetTypeId == relationType.TargetTypeId;
        }
    }
}
