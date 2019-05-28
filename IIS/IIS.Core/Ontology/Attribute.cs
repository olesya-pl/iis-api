using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace IIS.Core
{
    public class Attribute : IInstance
    {
        public AttributeClass Schema { get; }
        public long Id { get; }
        public object Value { get; }

        public Attribute(AttributeClass schema, object value = null, long id = 0)
        {
            Schema = schema ?? throw new ArgumentNullException(nameof(schema));
            Value = value;
            Id = id;
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
