﻿using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace IIS.Core.Ontology
{
    public abstract class Type
    {
        private readonly List<Type> _relatedTypes = new List<Type>();
        public IEnumerable<Type> RelatedTypes => _relatedTypes;

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

            return DirectParents.Any(parentType => parentType == type || parentType.IsSubtypeOf(type));
        }

        public bool IsSuperTypeOf(Type type)
        {
            if (type is null) throw new ArgumentNullException(nameof(type));

            return type.DirectParents.Any(parentType => parentType == this || parentType.IsSubtypeOf(this));
        }

        public void AddType(Type type)
        {
            _relatedTypes.Add(type);
        }
    }
}
