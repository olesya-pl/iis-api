﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Iis.Domain
{
    public abstract class Node
    {
        private readonly List<Node> _nodes;

        public Guid Id { get; set; }
        public NodeType Type { get; }
        public IEnumerable<Node> Nodes => _nodes;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        protected Node(Guid id, NodeType type, DateTime createdAt = default, DateTime updatedAt = default)
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
}