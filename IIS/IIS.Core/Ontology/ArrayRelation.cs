using System;
using System.Collections.Generic;

namespace IIS.Core
{
    public class ArrayRelation : IOntologyNode
    {
        private readonly List<Relation> _relations = new List<Relation>();

        public Constraint Schema { get; }
        public IEnumerable<Relation> Relations => _relations;

        public ArrayRelation(Constraint schema, IEnumerable<Relation> relations)
            : this(schema)
        {
            foreach (var relation in relations) AddRelation(relation);
        }

        public ArrayRelation(Constraint schema)
        {
            Schema = schema ?? throw new ArgumentNullException(nameof(schema));
            if (!schema.IsArray) throw new SingleValueUnsupportedException(schema.Name);
        }

        public void AddRelation(Relation relation)
        {
            if (Schema != relation.Schema) throw new Exception($"Incompatible schemas.");
            _relations.Add(relation);
        }

        // IOntologyNode
        public void AcceptVisitor(IOntologyVisitor visitor) => visitor.VisitArrayRelation(this);
        ISchemaNode IOntologyNode.Schema => Schema;
        IEnumerable<IOntologyNode> IOntologyNode.Nodes => Relations;
    }
}
