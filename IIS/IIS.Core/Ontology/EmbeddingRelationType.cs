using System;
using System.Collections.Generic;
using System.Linq;
using IIS.Core.Ontology.Meta;

namespace IIS.Core.Ontology
{
    public enum EmbeddingOptions { Optional, Required, Multiple }

    public class EmbeddingRelationType : RelationType
    {
        public bool IsInversed { get; }

        public override System.Type ClrType => typeof(Relation);

        public RelationMetaBase EmbeddingMeta => (RelationMetaBase) base.Meta;

        public EmbeddingOptions EmbeddingOptions { get; }

        // Embedding relation can have single attribute or single entity as a node
        public AttributeType AttributeType => RelatedTypes.OfType<AttributeType>().SingleOrDefault();
        public EntityType EntityType => RelatedTypes.OfType<EntityType>().SingleOrDefault();
        public Type TargetType => (Type)AttributeType ?? EntityType;
        public IEnumerable<RelationType> RelationTypes => RelatedTypes.OfType<RelationType>();
        public bool IsAttributeType => RelatedTypes.OfType<AttributeType>().Any();
        public bool IsEntityType => RelatedTypes.OfType<EntityType>().Any();
        public EmbeddingRelationType DirectRelationType => RelatedTypes.OfType<EmbeddingRelationType>().Single();

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
