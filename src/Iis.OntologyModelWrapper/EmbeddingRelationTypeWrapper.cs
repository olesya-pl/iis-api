using Iis.Domain;
using Iis.Domain.Meta;
using Iis.Interfaces.Meta;
using Iis.Interfaces.Ontology.Schema;
using Iis.OntologySchema.DataTypes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.OntologyModelWrapper
{
    public class EmbeddingRelationTypeWrapper : NodeTypeWrapper, IEmbeddingRelationTypeModel
    {
        public EmbeddingRelationTypeWrapper(INodeTypeLinked source) : base(source) { }

        public ISchemaMeta EmbeddingMeta => _source.MetaObject;

        public EmbeddingOptions EmbeddingOptions => _source.RelationType.EmbeddingOptions;

        public INodeTypeModel EntityType =>
            _source.RelationType.TargetType.Kind == Kind.Entity ?
                    new NodeTypeWrapper(_source.RelationType.TargetType) :
                    null;

        public INodeTypeModel AttributeType =>
            _source.RelationType.TargetType.Kind == Kind.Attribute ?
                new NodeTypeWrapper(_source.RelationType.TargetType) :
                null;

        public bool IsAttributeType => _source.RelationType.TargetType.Kind == Kind.Attribute;

        public bool IsEntityType => _source.RelationType.TargetType.Kind == Kind.Entity;

        public bool IsInversed => _source.IsInversed;

        public INodeTypeModel TargetType =>
            _source?.RelationType.TargetType == null ?
                null :
                new NodeTypeWrapper(_source.RelationType.TargetType);

        public bool AcceptsOperation(EntityOperation create) => true;

        public override string ToString() => Name;
    }
}
