using System;
using System.Collections.Generic;
using System.Linq;

namespace IIS.Core.Ontology
{
    public abstract class Node
    {
        private List<Node> _nodes = new List<Node>();

        public Guid Id { get; set; }
        public Type Type { get; }
        public IEnumerable<Node> Nodes => _nodes;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public Node(Guid id, Type type, DateTime createdAt = default, DateTime updatedAt = default)
        {
            Id = id;
            Type = type;
            CreatedAt = createdAt == default ? DateTime.UtcNow : createdAt; // todo: remove stubs
            UpdatedAt = updatedAt == default ? DateTime.UtcNow : updatedAt; // todo: remove stubs
        }

        public virtual void AddNode(Node node)
        {
            _nodes.Add(node);
        }

        public bool RemoveNode(Node node)
        {
            return _nodes.Remove(node);
        }

        public object GetAttributeValue(string relationName)
        {
            var relation = Nodes.SingleOrDefault(e => e.Type.Name == relationName);
            var attribute = relation?.Nodes.OfType<Attribute>().Single();
            return attribute?.Value;
        }

        public Entity GetEntity(string relationName)
        {
            var relation = Nodes.SingleOrDefault(e => e.Type.Name == relationName);
            var entity = relation?.Nodes.OfType<Entity>().Single();
            return entity;
        }

        public IEnumerable<Relation> GetRelations(RelationType relationType) =>
            Nodes.OfType<Relation>().Where(r => r.Type.Name == relationType.Name);

        // Only single relations
        public Relation GetRelation(RelationType relationType) => GetRelations(relationType).SingleOrDefault();

        // For single or multiple relations
        public Relation GetRelation(RelationType relationType, Guid targetId) =>
            GetRelations(relationType).SingleOrDefault(r => r.Target.Id == targetId);

        public override string ToString()
        {
            return $"Instance of type {Type.Name} with ID: {Id}.";
        }
    }

    public class Attribute : Node
    {
        public object Value { get; }

        public Attribute(Guid id, AttributeType type, object value, DateTime createdAt = default, DateTime updatedAt = default)
            : base(id, type, createdAt, updatedAt)
        {
            if (!type.AcceptsScalar(value)) throw new Exception("Inconsistency between attribute type and given object.");

            Value = value;
        }

        public override string ToString()
        {
            return $"{base.ToString()} Value: {Value.ToString()}";
        }
    }

    public class Entity : Node
    {
        public Entity(Guid id, EntityType type, DateTime createdAt = default, DateTime updatedAt = default)
            : base(id, type, createdAt, updatedAt)
        {

        }

        public override void AddNode(Node node)
        {
            var relationType = (RelationType)node.Type;
            if (relationType is EmbeddingRelationType)
            {
                var embeddingRelationType = (EmbeddingRelationType)relationType;
                if (embeddingRelationType.EmbeddingOptions != EmbeddingOptions.Multiple)
                {
                    var existingNode = Nodes.SingleOrDefault(e => e.Type == relationType);
                    if (existingNode != null) throw new Exception($"Relation {relationType} supports single value only.");
                }
            }

            base.AddNode(node);
        }
    }

    public class Relation : Node
    {
        public Node Target => Nodes.SingleOrDefault(e => e.Type.GetType() != typeof(RelationType))
                              ?? throw new Exception("Relation does not have a target.");

        public Attribute AttributeTarget => Nodes.OfType<Attribute>().Single();
        public Entity EntityTarget => Nodes.OfType<Entity>().Single();

        public Relation(Guid id, RelationType type, DateTime createdAt = default, DateTime updatedAt = default)
            : base(id, type, createdAt, updatedAt)
        {

        }

        public override string ToString()
        {
            return $"{base.ToString()} Aimed to: {Target.ToString()}";
        }
    }
}
