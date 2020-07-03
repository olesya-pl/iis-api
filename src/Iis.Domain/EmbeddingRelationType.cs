using System;
using System.Collections.Generic;
using System.Linq;
using Iis.Domain.Meta;
using Iis.Interfaces.Ontology.Schema;

namespace Iis.Domain
{
    public sealed class EmbeddingRelationType : RelationType, IEmbeddingRelationTypeModel
    {
        public bool IsInversed { get; }

        public override Type ClrType => typeof(Relation);

        public RelationMetaBase EmbeddingMeta => (RelationMetaBase)base.Meta;

        public EmbeddingOptions EmbeddingOptions { get; }

        // Embedding relation can have single attribute or single entity as a node
        public IAttributeTypeModel IAttributeTypeModel => RelatedTypes.OfType<IAttributeTypeModel>().SingleOrDefault();
        public EntityType EntityType => RelatedTypes.OfType<EntityType>().SingleOrDefault();
        public INodeTypeModel TargetType => (INodeTypeModel)IAttributeTypeModel ?? EntityType;
        public IEnumerable<IRelationTypeModel> RelationTypes => RelatedTypes.OfType<IRelationTypeModel>();
        public bool IsAttributeType => RelatedTypes.OfType<IAttributeTypeModel>().Any();
        public bool IsEntityType => RelatedTypes.OfType<EntityType>().Any();
        public IEmbeddingRelationTypeModel DirectRelationType => RelatedTypes.OfType<IEmbeddingRelationTypeModel>().Single();

        public EmbeddingRelationType(Guid id, string name, EmbeddingOptions embeddingOptions, bool isInversed = false)
            : base(id, name)
        {
            EmbeddingOptions = embeddingOptions;
            IsInversed = isInversed;
        }

        public override string ToString()
        {
            return $"{GetType()} '{Name}' to {TargetType}";
        }
    }
}
