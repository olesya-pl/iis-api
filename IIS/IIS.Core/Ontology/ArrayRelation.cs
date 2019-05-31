using System;
using System.Collections.Generic;

namespace IIS.Core
{
    public class ArrayRelation : IOntologyNode
    {
        private readonly List<IInstance> _instances = new List<IInstance>();

        public Constraint Schema { get; }
        public IEnumerable<IInstance> Instances => _instances;

        public ArrayRelation(Constraint schema, IEnumerable<IInstance> instances)
            : this(schema)
        {
            foreach (var instance in instances) AddInstance(instance);
        }

        public ArrayRelation(Constraint schema)
        {
            Schema = schema ?? throw new ArgumentNullException(nameof(schema));
            if (!schema.IsArray) throw new SingleValueUnsupportedException(schema.Name);
        }

        public void AddInstance(IInstance instance)
        {
            //if (Schema.Target != instance.Schema) throw new Exception($"Incompatible schemas.");
            _instances.Add(instance);
        }

        // IOntologyNode
        public void AcceptVisitor(IOntologyVisitor visitor) => visitor.VisitArrayRelation(this);
        ISchemaNode IOntologyNode.Schema => Schema;
        IEnumerable<IOntologyNode> IOntologyNode.Nodes => Instances;
    }
}
