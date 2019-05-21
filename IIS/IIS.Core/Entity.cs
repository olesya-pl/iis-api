using System;
using System.Collections.Generic;

namespace IIS.Core
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
            if (type.IsAbstract) throw new Exception("Cannot create instance of abstract type.");
            Id = id;
            // todo: remove from ctor
            _relations.Add("id", new AttributeRelation(Type.GetAttribute("id"), new AttributeValue(0, id)));
        }

        public bool IsTypeOf(TypeEntity type) => type.Name == Type.Name || type.Name == Type.Parent?.Name;

        public void AddRelation(Relation relation)
        {
            if (relation == null) throw new ArgumentNullException(nameof(relation));
            if (!Type.HasConstraint(relation.Name))
                throw new Exception($"Type {Type.Name} does not have constraint {relation.Name}.");
            _relations.Add(relation.Name, relation);
        }

        public Relation GetRelation(string name)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            if (!Type.HasConstraint(name))
                throw new Exception($"Type {Type.Name} does not have constraint {name}.");

            return _relations.GetValueOrDefault(name);
        }

        public object GetRelationTarget(string relationName)
        {
            var relation = GetRelation(relationName);
            if (relation != null) return relation.Target;
            return Type.GetConstraint(relationName).IsArray ? new object[0] : null;
        }
    }
}
