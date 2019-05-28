using System;
using System.Collections.Generic;

namespace IIS.Core
{
    public class Relation : IOntologyNode
    {
        public Constraint Schema { get; }
        public IInstance Target { get; }
        public IEnumerable<Attribute> Attributes { get; } = new Attribute[0];

        public Relation(Constraint schema, IInstance target)
        {
            Schema = schema ?? throw new ArgumentNullException(nameof(schema));
            Target = target ?? throw new ArgumentNullException(nameof(target));
            if (!target.IsTypeOf(schema.Target))
                throw new Exception($"Constraint {schema.Name} does not support target {target.Schema.Name}");
        }

        // IOntologyNode
        public void AcceptVisitor(IOntologyVisitor visitor) => visitor.VisitRelation(this);
        ISchemaNode IOntologyNode.Schema => Schema;
        IEnumerable<IOntologyNode> IOntologyNode.Nodes => Target.Nodes;
    }
}
