using System;
using System.Collections.Generic;
using System.Linq;

namespace IIS.Core
{
    public class Relation : IOntologyNode
    {
        public Constraint Schema { get; }
        public IInstance Target { get; private set; }

        public Relation(Constraint schema, IInstance target)
            : this(schema)
        {
            Target = target ?? throw new ArgumentNullException(nameof(target));
            if (!target.IsTypeOf(schema.Target))
                throw new Exception($"Constraint {schema.Name} does not support target {target.Schema.Name}");
        }

        public Relation(Constraint schema)
        {
            Schema = schema ?? throw new ArgumentNullException(nameof(schema));
        }

        public void SetTarget(IInstance target)
        {
            if (!target.IsTypeOf(Schema.Target))
                throw new Exception($"Constraint {Schema.Name} does not support target {target.Schema.Name}");
            Target = target;
        }

        // IOntologyNode
        public void AcceptVisitor(IOntologyVisitor visitor) => visitor.VisitRelation(this);
        ISchemaNode IOntologyNode.Schema => Schema;
        IEnumerable<IOntologyNode> IOntologyNode.Nodes => new[] { Target };
    }
}
