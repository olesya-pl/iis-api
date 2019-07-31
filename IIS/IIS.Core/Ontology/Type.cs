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
}
