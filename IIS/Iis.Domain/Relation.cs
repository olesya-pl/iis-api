using System;
using System.Linq;

namespace Iis.Domain
{
    public sealed class Relation : Node
    {
        public Node Target
        {
            get
            {
                return Nodes.SingleOrDefault(e => e.Type.GetType() != typeof(RelationType))
                       ?? throw new Exception("Relation does not have a target.");
            }
            set
            {
                var currentValue = Nodes.SingleOrDefault(e => e.Type.GetType() != typeof(RelationType));
                if (currentValue != null)
                    RemoveNode(currentValue);
                AddNode(value);
            }
        }

        public Attribute AttributeTarget => Nodes.OfType<Attribute>().Single();
        public Entity EntityTarget => Nodes.OfType<Entity>().Single();

        public Relation(Guid id, RelationType type, DateTime createdAt = default, DateTime updatedAt = default)
            : base(id, type, createdAt, updatedAt)
        {

        }

        public override string ToString()
        {
            return $"{base.ToString()} Aimed to: {Target.ToString()}";
        }
    }
}