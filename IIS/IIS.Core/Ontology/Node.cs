using System;
using System.Collections.Generic;
using System.Linq;

namespace IIS.Core.Ontology
{
    public abstract class Node
    {
        private List<Node> _nodes = new List<Node>();

        public Guid Id { get; }
        public Type Type { get; }
        public IEnumerable<Node> Nodes => _nodes;
        public DateTime CreatedAt { get; }
        public DateTime UpdatedAt { get; }

        public Node(Guid id, Type type, DateTime createdAt = default, DateTime updatedAt = default)
        {
            Id = id;
            Type = type;
            CreatedAt = createdAt;
            UpdatedAt = updatedAt;
        }

        public void AddNode(Node node)
        {
            _nodes.Add(node);
        }

        public object GetAttributeValue(string relationName)
        {
            var relation = Nodes.SingleOrDefault(e => e.Type.Name == relationName);
            var attribute = relation?.Nodes.OfType<Attribute>().Single();
            return attribute.Value;
        }

        public Entity GetEntity(string relationName)
        {
            var relation = Nodes.SingleOrDefault(e => e.Type.Name == relationName);
            var entity = relation?.Nodes.OfType<Entity>().Single();
            return entity;
        }
    }

    public class Attribute : Node
    {
        public object Value { get; }

        public Attribute(Guid id, AttributeType type, object value, DateTime createdAt = default, DateTime updatedAt = default)
            : base(id, type, createdAt, updatedAt)
        {
            Value = value;
        }
    }

    public class Entity : Node
    {
        public Entity(Guid id, EntityType type, DateTime createdAt = default, DateTime updatedAt = default)
            : base(id, type, createdAt, updatedAt)
        {

        }
    }

    public class Relation : Node
    {
        public Relation(Guid id, RelationType type, DateTime createdAt = default, DateTime updatedAt = default)
            : base(id, type, createdAt, updatedAt)
        {

        }
    }
}
