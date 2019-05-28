using System;
using System.Collections.Generic;

namespace IIS.Core
{
    public class Constraint : ISchemaNode
    {
        public string Name { get; }
        public bool IsRequired { get; }
        public bool IsArray { get; }
        public IRelationResolver Resolver { get; }
        public IType Target { get; }

        public Constraint(string name, IType target, bool isRequired, bool isArray, IRelationResolver resolver = null)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Target = target ?? throw new ArgumentNullException(nameof(target));
            IsRequired = isRequired;
            IsArray = isArray;
            Resolver = resolver;
        }

        // ISchemaNode
        public void AcceptVisitor(ISchemaVisitor visitor) => visitor.VisitConstraint(this);
        IEnumerable<ISchemaNode> ISchemaNode.Nodes => new[] { Target };
    }
}
