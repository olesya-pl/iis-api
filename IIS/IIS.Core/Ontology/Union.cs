using System;
using System.Collections.Generic;

namespace IIS.Core
{
    public class Union : IInstance
    {
        private readonly List<Entity> _entities = new List<Entity>();

        public UnionClass Schema { get; }
        public IEnumerable<Entity> Objects => _entities;

        public Union(UnionClass schema)
        {
            Schema = schema ?? throw new ArgumentNullException(nameof(schema));
        }

        public void AddEntity(Entity entity) => _entities.Add(entity);

        // IInstance
        public bool IsTypeOf(IType schema) => Schema.Name == schema.Name;
        IType IInstance.Schema => Schema;

        // IOntologyNode
        public void AcceptVisitor(IOntologyVisitor visitor) => visitor.VisitUnion(this);
        ISchemaNode IOntologyNode.Schema => Schema;
        IEnumerable<IOntologyNode> IOntologyNode.Nodes => Objects;
    }
}
