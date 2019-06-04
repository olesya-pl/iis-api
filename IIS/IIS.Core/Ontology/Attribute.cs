using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace IIS.Core
{
    public class Attribute : IInstance
    {
        public AttributeClass Schema { get; }
        public Guid Id { get; }
        public object Value { get; }

        public Attribute(AttributeClass schema, object value = null, Guid? id = null)
        {
            Schema = schema ?? throw new ArgumentNullException(nameof(schema));
            Value = value;
            Id = id == null ? Guid.Empty : id.Value;
        }

        // IInstance
        public bool IsTypeOf(IType schema) => Schema.Name == schema.Name;
        IType IInstance.Schema => Schema;

        // IOntologyNode
        public void AcceptVisitor(IOntologyVisitor visitor) => visitor.VisitAttribute(this);
        ISchemaNode IOntologyNode.Schema => Schema;
        IEnumerable<IOntologyNode> IOntologyNode.Nodes => Enumerable.Empty<IOntologyNode>();
    }
}
