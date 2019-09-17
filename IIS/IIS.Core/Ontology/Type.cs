using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace IIS.Core.Ontology
{
    public abstract class Type
    {
        private readonly List<Type> _relatedTypes = new List<Type>();
        public IEnumerable<Type> RelatedTypes => _relatedTypes;

        public abstract System.Type ClrType { get; }

        public Guid Id { get; }
        public string Name { get; }
        public string Title { get; set; }
        public JObject Meta { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // todo: move to extensions?

        // tood: change to Type
        public IEnumerable<EntityType> DirectParents =>
            RelatedTypes.OfType<InheritanceRelationType>().Select(r => r.ParentType);

        public IEnumerable<EntityType> AllParents =>
            DirectParents.SelectMany(e => e.AllParents).Union(DirectParents);

        public IEnumerable<EmbeddingRelationType> DirectProperties =>
            RelatedTypes.OfType<EmbeddingRelationType>();

        public IEnumerable<EmbeddingRelationType> AllProperties =>
            AllParents.SelectMany(p => p.DirectProperties)
                .Where(pp => DirectProperties.All(dp => dp.Name != pp.Name)) // Ignore parent properties with same name (overriden)
                .Union(DirectProperties);

        public Type(Guid id, string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("The given string should not be empty or null", nameof(name));

            Id = id;
            Name = name;
        }

        public bool IsSubtypeOf(Type type)
        {
            if (type is null) throw new ArgumentNullException(nameof(type));
            if (Id == type.Id) return true;
            foreach (var inheritance in RelatedTypes.OfType<InheritanceRelationType>())
            {
                var parent = inheritance.ParentType;
                if (parent.IsSubtypeOf(type)) return true;
            }
            return false;
        }

        public void AddType(Type type)
        {
            _relatedTypes.Add(type);
        }

        public EmbeddingRelationType GetProperty(string typeName) =>
            AllProperties.SingleOrDefault(p => p.Name == typeName);
    }
}
