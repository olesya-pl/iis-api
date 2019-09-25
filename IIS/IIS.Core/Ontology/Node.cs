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

        public IEnumerable<Relation> GetRelations(RelationType relationType) =>
            Nodes.OfType<Relation>().Where(r => r.Type.Name == relationType.Name);

        public EmbeddingRelationType GetRelationType(string relationTypeName) =>
            Type.GetProperty(relationTypeName) ??
            throw new ArgumentException($"Relation with name {relationTypeName} does not exist");

        // Only single relations
        public Relation GetRelation(EmbeddingRelationType relationType) =>
            GetRelations(relationType).SingleOrDefault()
            ?? throw new ArgumentException($"There is no relation from {Type.Name} {Id} to {relationType.Name} of type {relationType.TargetType.Name}");

        public Relation GetRelationOrDefault(EmbeddingRelationType relationType) =>
            GetRelations(relationType).SingleOrDefault();

        // For single or multiple relations
        public Relation GetRelation(EmbeddingRelationType relationType, Guid relationId) =>
            GetRelations(relationType).SingleOrDefault(r => r.Id == relationId)
            ?? throw new ArgumentException($"There is no relation from {Type.Name} {Id} to {relationType.Name} of type {relationType.TargetType.Name} with id {relationId}");

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
                    if (existingNode != null) throw new Exception($"Relation '{relationType.Name}' supports single value only.");
                }
            }

            base.AddNode(node);
        }

        public object GetProperty(string relationName)
        {
            var embed = GetRelationType(relationName);
            var nodes = GetRelations(embed).Select(r => r.Target);
            if (embed.EmbeddingOptions == EmbeddingOptions.Multiple)
            {
                if (embed.IsAttributeType)
                    return nodes.Cast<Attribute>().Select(a => a.Value).ToList();
                return nodes.ToList();
            }
            var node = nodes.SingleOrDefault();
            if (node != null && embed.IsAttributeType)
                return ((Attribute) node).Value;
            return node;
        }

        public void SetProperty(string relationName, object value)
        {
            var embed = GetRelationType(relationName);
            var existingRelations = GetRelations(embed).ToList();
            List<object> targets;
            if (value == null)
            {
                targets = new List<object>(); // empty list of targets
            }
            else if (embed.EmbeddingOptions == EmbeddingOptions.Multiple)
            {
                if (!(value is IEnumerable<object> enumerable))
                    throw new ArgumentException($"Can not assign single value to multiple relation {Type.Name}.{embed.Name}");
                targets = enumerable.ToList();
            }
            else
            {
                targets = new List<object>{ value }; // Wrap single value in list
            }

            if (embed.IsAttributeType && targets.Any(e => e is Node))
                throw new ArgumentException($"Unable to set Node value to attribute {Type.Name}.{embed.Name}");
            if (embed.IsEntityType && targets.Any(e => !(e is Entity || e is Guid)))
                throw new ArgumentException($"Unable to set non-Entity and non-Guid value to attribute {Type.Name}.{embed.Name}");

            var targetNodes = embed.IsAttributeType
                ? targets.Select(t => new Attribute(Guid.NewGuid(), embed.AttributeType, t))
                : targets.Select(t => t is Guid guid
                    ? new Entity(guid, embed.EntityType) // Convert guids to node instances
                    : t).Cast<Node>();
            var newRelations = targetNodes.Select(n => new Relation(Guid.NewGuid(), embed) {Target = n});

            foreach (var r in existingRelations)
                RemoveNode(r);
            foreach (var r in newRelations)
                AddNode(r);
        }
    }

    public class Relation : Node
    {
        public Node Target
        {
            get
            {
                return Nodes.SingleOrDefault(e => e.Type.GetType() != typeof(RelationType))
                       ?? throw new Exception("Relation does not have a target.");
            }
            set
            {
                var currentValue = Nodes.SingleOrDefault(e => e.Type.GetType() != typeof(RelationType));
                if (currentValue != null)
                    RemoveNode(currentValue);
                AddNode(value);
            }
        }

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
