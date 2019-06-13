using System;
using System.Collections.Generic;
using System.Linq;

namespace IIS.Core
{
    public class AttributeClass : IType
    {
        public ScalarType Type { get; }

        public AttributeClass(string name, ScalarType type)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Type = type ?? throw new ArgumentNullException(nameof(type));
        }

        // IType
        public string Name { get; }
        public Kind Kind => Kind.Attribute;

        // ISchemaNode
        public void AcceptVisitor(ISchemaVisitor visitor) => visitor.VisitAttributeClass(this);
        IEnumerable<ISchemaNode> ISchemaNode.Nodes => Enumerable.Empty<ISchemaNode>();
    }
}
