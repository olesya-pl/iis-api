using System;
using System.Collections.Generic;
using System.Linq;
using Iis.Domain.Meta;
using Iis.Interfaces.Meta;
using Iis.Interfaces.Ontology.Schema;
using Newtonsoft.Json.Linq;

namespace Iis.Domain
{
    public abstract class NodeType
    {
        private readonly List<NodeType> _relatedTypes;

        public IEnumerable<NodeType> RelatedTypes => _relatedTypes;

        public abstract Type ClrType { get; }

        public Guid Id { get; }
        public string Name { get; }
        public string Title { get; set; }
        public IMeta Meta { get; set; }
        public JObject MetaSource { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool HasUniqueValues => UniqueValueFieldName != null;
        public string UniqueValueFieldName { get; set; }

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

        public bool IsObjectOfStudy => 
            AllParents.Any(p => p.Name == EntityTypeNames.ObjectOfStudy.ToString());

        protected NodeType(Guid id, string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("The given string should not be empty or null", nameof(name));

            _relatedTypes = new List<NodeType>();

            Id = id;
            Name = name;
        }

        public bool IsSubtypeOf(NodeType type)
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

        public void AddType(NodeType type)
        {
            _relatedTypes.Add(type);
        }

        public EmbeddingRelationType GetProperty(string typeName) =>
            AllProperties.SingleOrDefault(p => p.Name == typeName);

        public override string ToString()
        {
            return $"{GetType()} '{Name}'";
        }
    }
}
