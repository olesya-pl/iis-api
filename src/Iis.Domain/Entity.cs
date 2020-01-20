using System;
using System.Collections.Generic;
using System.Linq;

namespace Iis.Domain
{
    public sealed class Entity : Node
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
}