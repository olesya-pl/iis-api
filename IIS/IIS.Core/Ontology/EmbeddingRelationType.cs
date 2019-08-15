﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace IIS.Core.Ontology
{
    public enum EmbeddingOptions { Optional, Required, Multiple }
    public class EmbeddingRelationType : RelationType
    {
        public EmbeddingOptions EmbeddingOptions { get; }

        // Embedding relation can have single attribute or single entity as a node
        public AttributeType AttributeType => RelatedTypes.OfType<AttributeType>().SingleOrDefault();
        public EntityType EntityType => RelatedTypes.OfType<EntityType>().SingleOrDefault();
        public Type TargetType => (Type)AttributeType ?? EntityType;
        public IEnumerable<RelationType> RelationTypes => RelatedTypes.OfType<RelationType>();
        public bool IsAttributeType => RelatedTypes.OfType<AttributeType>().Any();
        public bool IsEntityType => RelatedTypes.OfType<EntityType>().Any();

        public EmbeddingRelationType(Guid id, string name, EmbeddingOptions embeddingOptions)
            : base(id, name)
        {
            EmbeddingOptions = embeddingOptions;
        }
    }
}
