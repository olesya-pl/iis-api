using System;
using System.Collections.Generic;

namespace IIS.Core
{
    public class UnionClass : IType
    {
        private readonly Dictionary<string, TypeEntity> _targets = new Dictionary<string, TypeEntity>();

        public IEnumerable<TypeEntity> Classes => _targets.Values;

        public UnionClass(string name, IEnumerable<TypeEntity> types)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            foreach (var type in types) _targets.Add(type.Name, type);
        }

        public void AddType(TypeEntity type) => _targets.Add(type.Name, type);

        // IType
        public string Name { get; }
        public Kind Kind => Kind.Union;

        // SchemaNode
        public void AcceptVisitor(ISchemaVisitor visitor) => visitor.VisitUnionClass(this);
        IEnumerable<ISchemaNode> ISchemaNode.Nodes => Classes;
    }
}
