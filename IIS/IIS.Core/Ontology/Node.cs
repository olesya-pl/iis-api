using System;
using System.Collections.Generic;
using System.Linq;

namespace IIS.Core.Ontology
{
    public abstract class Node
    {
        private Dictionary<string, Node> _nodes = new Dictionary<string, Node>();

        public Guid Id { get; }
        public Type Type { get; }
        public IEnumerable<Node> Nodes => _nodes.Values;
        public DateTime CreatedAt { get; }
        public DateTime UpdatedAt { get; }

        public Node(Guid id, DateTime createdAt = default, DateTime updatedAt = default)
        {
            Id = id;
            CreatedAt = createdAt;
            UpdatedAt = updatedAt;
        }

        public void AddNode(Node node)
        {
            _nodes.Add(node.Type.Name, node);
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

        public Attribute(Guid id, object value, DateTime createdAt = default, DateTime updatedAt = default)
            : base(id, createdAt, updatedAt)
        {
            Value = value;
        }
    }

    public class Entity : Node
    {
        public Entity(Guid id, DateTime createdAt = default, DateTime updatedAt = default)
            : base(id, createdAt, updatedAt)
        {

        }
    }

    public class Relation : Node
    {
        public Relation(Guid id, DateTime createdAt = default, DateTime updatedAt = default)
            : base(id, createdAt, updatedAt)
        {

        }
    }
}
