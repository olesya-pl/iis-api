using Iis.Interfaces.Ontology.Data;
using Iis.Interfaces.Ontology.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Iis.Domain
{
    public abstract class Node
    {
        private readonly List<Node> _nodes;

        public Guid Id { get; set; }
        public INodeTypeLinked Type { get; }
        public IEnumerable<Node> Nodes => _nodes;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public INode OriginalNode { get; set; }

        protected Node(Guid id, INodeTypeLinked type, DateTime createdAt = default, DateTime updatedAt = default)
        {
            _nodes = new List<Node>();

            Id = id;
            Type = type ?? throw new ArgumentNullException(nameof(type));
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

        public IEnumerable<Relation> GetRelations(string relationTypeName) =>
            Nodes.OfType<Relation>().Where(r => r.Type.Name == relationTypeName);

        public INodeTypeLinked GetRelationType(string relationTypeName) =>
            Type.GetProperty(relationTypeName) ??
            throw new ArgumentException($"Relation with name {relationTypeName} does not exist");

        // Only single relations
        public Relation GetRelation(INodeTypeLinked relationType) =>
            GetRelations(relationType.Name).SingleOrDefault()
            ?? throw new ArgumentException($"There is no relation from {Type.Name} {Id} to {relationType.Name} of type {relationType.TargetType.Name}");

        public Relation GetRelationOrDefault(INodeTypeLinked relationType) =>
            GetRelations(relationType.Name).SingleOrDefault();

        // For single or multiple relations
        public Relation GetRelation(INodeTypeLinked relationType, Guid relationId) =>
            GetRelations(relationType.Name).SingleOrDefault(r => r.Id == relationId)
            ?? throw new ArgumentException($"There is no relation from {Type.Name} {Id} to {relationType.Name} of type {relationType.TargetType.Name} with id {relationId}");

        public override string ToString()
        {
            return $"Instance of type {Type.Name} with ID: {Id}.";
        }

        public List<(Attribute attribute, string dotName)> GetChildAttributes()
        {
            var result = new List<(Attribute, string)>();
            if (this is Attribute)
            {
                result.Add((this as Attribute, null));
            }
            foreach (var relation in Nodes)
            {
                var children = relation.GetChildAttributes();
                foreach (var child in children)
                {
                    result.Add((child.attribute, GetDotName(child)));
                }
            }
            return result;
        }

        public List<(Attribute attribute, string dotName)> GetTopLevelAttributes()
        {
            var result = new List<(Attribute, string)>();
            if (this is Attribute)
            {
                result.Add((this as Attribute, Type.Name));
            }
            foreach (var node in Nodes)
            {
                if (node is Attribute)
                {
                    result.Add((node as Attribute, Type.Name));
                }
                if (node is Relation)
                {
                    var relation = node as Relation;
                    if (relation.AttributeTarget == null)
                    {
                        continue;
                    }
                    result.Add((relation.AttributeTarget, relation.AttributeTarget.Type.Name));
                }
            }
            return result;
        }

        private string GetDotName((Attribute attribute, string dotName) child)
        {
            var sb = new StringBuilder();
            if (this.Type.Kind == Kind.Relation)
            {
                sb.Append(this.Type.Name);
            }

            if (child.dotName != null)
            {
                if (sb.Length > 0)
                {
                    sb.Append(".");
                }
                sb.Append(child.dotName);
            }

            return sb.ToString();
        }

        public List<(Attribute Attribute, string DotName)> GetChildAttributesExcludingNestedObjects()
        {
            var result = new List<(Attribute, string)>();
            if (this is Attribute)
            {
                result.Add((this as Attribute, null));
            }
            foreach (var relation in Nodes.Where(n => !n.Type.IsObject))
            {
                var children = relation.GetChildAttributesExcludingNestedObjects();
                foreach (var child in children)
                {
                    result.Add((child.Attribute, GetDotName(child)));
                }
            }
            return result;
        }
    }
}
