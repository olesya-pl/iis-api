using System;
using System.Collections.Generic;
using System.Linq;
using Iis.Domain.Meta;
using Iis.Interfaces.Meta;
using Iis.Interfaces.Ontology.Schema;
using Newtonsoft.Json.Linq;

namespace Iis.Domain
{
    public abstract class NodeType : INodeTypeModel
    {
        private readonly List<INodeTypeModel> _relatedTypes = new List<INodeTypeModel>();

        public IEnumerable<INodeTypeModel> RelatedTypes => _relatedTypes;

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
        public IEnumerable<IEntityTypeModel> DirectParents =>
            RelatedTypes.OfType<IInheritanceRelationTypeModel>().Select(r => r.ParentType);

        public IEnumerable<IEntityTypeModel> AllParents =>
            DirectParents.SelectMany(e => e.AllParents).Union(DirectParents);

        public IEnumerable<IEmbeddingRelationTypeModel> DirectProperties =>
            RelatedTypes.OfType<IEmbeddingRelationTypeModel>();

        public IEnumerable<IEmbeddingRelationTypeModel> AllProperties =>
            AllParents.SelectMany(p => p.DirectProperties)
                .Where(pp => DirectProperties.All(dp => dp.Name != pp.Name)) // Ignore parent properties with same name (overriden)
                .Union(DirectProperties);

        public bool IsObjectOfStudy =>
            AllParents.Any(p => p.Name == EntityTypeNames.ObjectOfStudy.ToString());

        public INodeTypeLinked Source => throw new NotImplementedException();

        protected NodeType(Guid id, string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("The given string should not be empty or null", nameof(name));

            _relatedTypes = new List<INodeTypeModel>();

            Id = id;
            Name = name;
        }

        public bool IsSubtypeOf(INodeTypeModel type)
        {
            if (type is null) throw new ArgumentNullException(nameof(type));
            if (Id == type.Id) return true;
            foreach (var inheritance in RelatedTypes.OfType<IInheritanceRelationTypeModel>())
            {
                var parent = inheritance.ParentType;
                if (parent.IsSubtypeOf(type)) return true;
            }
            return false;
        }

        public void AddType(INodeTypeModel type)
        {
            if (this.Name == "aaa")
                _relatedTypes.Add(type);
            else
                _relatedTypes.Add(type);
        }

        public IEmbeddingRelationTypeModel GetProperty(string typeName) =>
            AllProperties.SingleOrDefault(p => p.Name == typeName);

        public override string ToString()
        {
            return $"{GetType()} '{Name}'";
        }
    }
}
