using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace IIS.Core
{
    public class Entity : IInstance
    {
        private readonly Dictionary<string, IOntologyNode> _relations = new Dictionary<string, IOntologyNode>();

        public TypeEntity Schema { get; }
        public long Id => (long)((Attribute)GetSingleRelation("id").Target).Value; // ???
        public IEnumerable<string> RelationNames => _relations.Keys;

        public Entity(TypeEntity schema)
        {
            Schema = schema ?? throw new ArgumentNullException(nameof(schema));
            if (schema.IsAbstract) throw new Exception("Cannot create instance of abstract type.");
            foreach (var constraint in schema.Constraints.Where(c => c.IsArray))
                _relations.Add(constraint.Name, new ArrayRelation(constraint));
        }

        public bool IsTypeOf(TypeEntity schema) => schema.Name == Schema.Name || schema.Name == Schema.Parent?.Name;
        bool IInstance.IsTypeOf(IType schema) => IsTypeOf((TypeEntity)schema);

        public bool HasRelation(string name)
        {
            if (!Schema.HasConstraint(name)) throw new ConstraintNotFoundException(Schema.Name, name);

            return _relations.ContainsKey(name);
        }

        public IOntologyNode GetRelation(string name) => HasRelation(name) ? _relations[name] : null;

        public Relation GetSingleRelation(string name) => HasRelation(name) ? (Relation)_relations[name] : null;

        public ArrayRelation GetArrayRelation(string name) => (ArrayRelation)_relations[name];

        public void SetRelation(Relation relation)
        {
            var name = relation.Schema.Name;
            if (Schema.GetConstraint(name) != relation.Schema) throw new Exception($"Incompatible schemas.");
            if (relation.Schema.IsArray) throw new ArrayUnsupportedException(Schema.Name, name);

            _relations[name] = relation;
        }

        public void AddRelation(Relation relation)
        {
            var name = relation.Schema.Name;
            if (Schema.GetConstraint(name) != relation.Schema) throw new Exception($"Incompatible schemas.");
            if (!relation.Schema.IsArray) throw new SingleValueUnsupportedException(name);

            var arrayRelation = (ArrayRelation)_relations[name];
            arrayRelation.AddRelation(relation);
        }

        // IInstance
        IType IInstance.Schema => Schema;

        // IOntologyNode
        public void AcceptVisitor(IOntologyVisitor visitor) => visitor.VisitObject(this);
        ISchemaNode IOntologyNode.Schema => Schema;
        public IEnumerable<IOntologyNode> Nodes => _relations.Values;
    }
}
