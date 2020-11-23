using System;
using System.Collections.Generic;
using System.Linq;

namespace Iis.Domain
{
    public sealed class Relation : Node
    {
        public Node Target
        {
            get
            {
                return Nodes.SingleOrDefault(e => e.Type.GetType() != typeof(IRelationTypeModel))
                       ?? throw new Exception("Relation does not have a target.");
            }
            set
            {
                var currentValue = Nodes.SingleOrDefault(e => e.Type.GetType() != typeof(IRelationTypeModel));
                if (currentValue != null)
                    RemoveNode(currentValue);
                AddNode(value);
            }
        }

        public Attribute AttributeTarget => Nodes.OfType<Attribute>().SingleOrDefault();
        public Entity EntityTarget => Nodes.OfType<Entity>().SingleOrDefault();

        public Relation(Guid id, INodeTypeModel type, DateTime createdAt = default, DateTime updatedAt = default)
            : base(id, type, createdAt, updatedAt)
        {

        }

        public override string ToString()
        {
            return $"{base.ToString()} Aimed to: {Target.ToString()}";
        }
    }
}