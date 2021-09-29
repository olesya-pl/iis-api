using Iis.Interfaces.Ontology.Schema;
using System;

namespace Iis.Domain
{
    public sealed class Attribute : Node
    {
        public object Value { get; }

        public Attribute(Guid id, INodeTypeLinked type, object value, DateTime createdAt = default, DateTime updatedAt = default)
            : base(id, type, createdAt, updatedAt)
        {
            if (!type.AcceptsScalar(value)) 
                throw new Exception("Inconsistency between attribute type and given object.");

            Value = value;
        }

        public override string ToString()
        {
            return $"{base.ToString()} Value: {Value}";
        }
    }
}