using Iis.Domain;
using Iis.Domain.Meta;
using Iis.Interfaces.Meta;
using Iis.Interfaces.Ontology.Schema;
using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.OntologyModelWrapper
{
    public class EmbeddingRelationTypeWrapper : RelationTypeWrapper, IEmbeddingRelationTypeModel
    {
        public EmbeddingRelationTypeWrapper(INodeTypeLinked source) : base(source) { }
        public IEmbeddingRelationTypeModel DirectRelationType =>
            _source.RelationType.DirectRelationType?.NodeType == null ?
                null : 
                new EmbeddingRelationTypeWrapper(_source.RelationType.DirectRelationType?.NodeType); 

        public IRelationMetaBase EmbeddingMeta => _source.MetaMeta;

        public EmbeddingOptions EmbeddingOptions => _source.RelationType.EmbeddingOptions;

        public IEntityTypeModel EntityType =>
            _source.RelationType.TargetType.Kind == Kind.Entity ?
                    new EntityTypeWrapper(_source.RelationType.TargetType) :
                    null;

        public IAttributeTypeModel AttributeType =>
            _source.RelationType.TargetType.Kind == Kind.Attribute ?
                new AttributeTypeWrapper(_source.RelationType.TargetType) :
                null;

        public bool IsAttributeType => _source.RelationType.TargetType.Kind == Kind.Attribute;

        public bool IsEntityType => _source.RelationType.TargetType.Kind == Kind.Entity;

        public bool IsInversed => _source.IsInversed;

        public INodeTypeModel TargetType =>
            _source?.RelationType.TargetType == null ?
                null :
                new AttributeTypeWrapper(_source.RelationType.TargetType);

        public bool AcceptsOperation(EntityOperation create) => false;

        public override string ToString() => Name;
    }
}
