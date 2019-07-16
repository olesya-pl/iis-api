using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace IIS.Core.Ontology
{
    public abstract class Type
    {
        private readonly List<Type> _nodes = new List<Type>();
        public IEnumerable<Type> Nodes => _nodes;

        public Guid Id { get; }
        public string Name { get; }
        public string Title { get; }
        public JObject Meta { get; }
        public DateTime CreatedAt { get; }
        public DateTime UpdatedAt { get; }

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
        public AttributeType(Guid id, string name)
            : base(id, name)
        {

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

    public class EmbeddingRelationType : RelationType
    {
        public bool IsRequired;
        public bool IsArray;

        public EmbeddingRelationType(Guid id, string name, bool isRequired, bool isArray)
            : base(id, name)
        {
            IsRequired = isRequired;
            IsArray = isArray;
        }
    }

    public class InheritanceRelationType : RelationType
    {
        public InheritanceRelationType(Guid id)
            : base(id, "Is")
        {

        }
    }
}
