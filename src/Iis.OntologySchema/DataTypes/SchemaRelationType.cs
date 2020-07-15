using Iis.Interfaces.Ontology.Schema;
using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.OntologySchema.DataTypes
{
    public class SchemaRelationType: SchemaRelationTypeRaw, IRelationType, IRelationTypeLinked
    {
        internal SchemaNodeType _nodeType;
        internal SchemaNodeType _sourceType;
        internal SchemaNodeType _targetType;
        public INodeTypeLinked INodeTypeModel => _nodeType;
        public INodeTypeLinked SourceType => _sourceType;
        public INodeTypeLinked TargetType => _targetType;
        public override string ToString()
        {
            return $"{SourceType.Name}.{INodeTypeModel.Name}";
        }
        public bool IsIdentical(IRelationTypeLinked relationType, bool includeTargetType)
        {
            return Kind == relationType.Kind
                && EmbeddingOptions == relationType.EmbeddingOptions
                && SourceType.Name == relationType.SourceType.Name
                && TargetType.Name == relationType.TargetType.Name
                && (!includeTargetType || TargetType.IsIdentical(relationType.TargetType));
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
