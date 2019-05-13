using System;
using System.Collections.Generic;

namespace IIS.OSchema
{
    public class Entity
    {
        private readonly Dictionary<string, Relation> _relations = new Dictionary<string, Relation>();

        public TypeEntity Type { get; }
        public long Id { get; }
        public IEnumerable<Relation> Relations => _relations.Values;

        public Entity(TypeEntity type, long id)
        {
            Type = type ?? throw new ArgumentNullException(nameof(type));
            Id = id;
        }

        public Entity(TypeEntity type, long id, IEnumerable<Relation> relations)
            : this(type, id)
        {
            if (relations == null) throw new ArgumentNullException(nameof(relations));
            foreach (var relation in relations) AddRelation(relation);
        }

        public Relation this[string name]
        {
            get => Type.HasConstraint(name) ? _relations[name] 
                : throw new Exception($"Type {Type.Name} does not have constraint {name}.");
            set => _relations[name] = Type.HasConstraint(name) ? value
                : throw new Exception($"Type {Type.Name} does not have constraint {name}.");
        }

        public void AddRelation(Relation relation)
        {
            if (relation == null) throw new ArgumentNullException(nameof(relation));
            if (!Type.HasConstraint(relation.Name))
                throw new Exception($"Type {Type.Name} does not have constraint {relation.Name}.");
            _relations.Add(relation.Name, relation);
        }
    }
}
