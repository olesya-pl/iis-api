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
        private SchemaNodeType _nodeType;
        private SchemaNodeType _sourceType;
        private SchemaNodeType _targetType;
        public INodeTypeLinked NodeType => _nodeType;
        public INodeTypeLinked SourceType => _sourceType;
        public INodeTypeLinked TargetType => _targetType;
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

        internal void SetNodeType(SchemaNodeType nodeType)
        {
            _nodeType = nodeType;
        }

        internal void SetSourceType(SchemaNodeType sourceType)
        {
            _sourceType = sourceType;
        }

        internal void SetTargetType(SchemaNodeType targetType)
        {
            _targetType = targetType;
        }

        internal void SetMeta(string meta)
        {
            _nodeType.Meta = meta;
        }
    }
}
