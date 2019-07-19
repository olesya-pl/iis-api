using System;
using System.Collections.Generic;
using System.Linq;
using HotChocolate.Types.Relay.Descriptors;
using Newtonsoft.Json.Linq;

namespace IIS.Core.Ontology
{
    public abstract class Type
    {
        private readonly List<Type> _nodes = new List<Type>();
        public IEnumerable<Type> Nodes => _nodes;

        public Guid Id { get; }
        public string Name { get; }
        public string Title { get; set; }
        public JObject Meta { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        
        
        public IEnumerable<EntityType> DirectParents =>
            Nodes.OfType<InheritanceRelationType>().Select(r => r.ParentType);

        public IEnumerable<EntityType> AllParents =>
            DirectParents.SelectMany(e => e.AllParents).Union(DirectParents);

        public IEnumerable<EmbeddingRelationType> DirectProperties =>
            Nodes.OfType<EmbeddingRelationType>();

        public IEnumerable<EmbeddingRelationType> AllProperties =>
            AllParents.SelectMany(p => p.DirectProperties).Union(DirectProperties);
        
        public Type(Guid id, string name)
        {
            Id = id;
            Name = name;
        }

        public void AddType(Type type)
        {
            _nodes.Add(type);
        }
    }

    public class AttributeType : Type
    {
        public string ScalarType { get; }

        public AttributeType(Guid id, string name, string scalarType)
            : base(id, name)
        {
            ScalarType = scalarType;
        }
    }

    public class EntityType : Type
    {
        public bool IsAbstract { get; }
        public bool IsMarker => !Nodes.Any();

        public EntityType(Guid id, string name, bool isAbstract)
            : base(id, name)
        {
            IsAbstract = isAbstract;
        }
    }

    public abstract class RelationType : Type
    {
        protected RelationType(Guid id, string name)
            : base(id, name)
        {

        }
    }

    public enum EmbeddingOptions { Optional, Required, Multiple }
    public class EmbeddingRelationType : RelationType
    {
        public EmbeddingOptions EmbeddingOptions { get; }

        // Embedding relation can have single attribute or single entity as a node
        public AttributeType AttributeType => Nodes.OfType<AttributeType>().SingleOrDefault(); 
        public EntityType EntityType => Nodes.OfType<EntityType>().SingleOrDefault();
        public Type TargetType => (Type) AttributeType ?? EntityType;
        public IEnumerable<RelationType> RelationTypes => Nodes.OfType<RelationType>();
        public bool IsAttributeType => Nodes.OfType<AttributeType>().Any();
        public bool IsEntityType => Nodes.OfType<EntityType>().Any();

        public EmbeddingRelationType(Guid id, string name, EmbeddingOptions embeddingOptions)
            : base(id, name)
        {
            EmbeddingOptions = embeddingOptions;
        }
    }

    public class InheritanceRelationType : RelationType
    {
        public EntityType ParentType => Nodes.OfType<EntityType>().Single(); // Inheritance relation should always have single EntityType node (parent)
        
        public InheritanceRelationType(Guid id)
            : base(id, "Is")
        {

        }
    }
}
