using Iis.Interfaces.Ontology.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Iis.Api.Ontology.Migration
{
    public class SnapshotAttributeType
    {
        public Guid Id { get; set; }
        public ScalarType ScalarType { get; set; }
    }
    public class SnatshotRelationType
    {
        public Guid Id { get; set; }
        public RelationKind Kind { get; set; }
        public EmbeddingOptions EmbeddingOptions { get; set; }
        public Guid SourceTypeId { get; set; }
        public Guid TargetTypeId { get; set; }
    }
    public class SnapshotNodeType
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Title { get; set; }
        public string Meta { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsArchived { get; set; }
        public Kind Kind { get; set; }
        public bool IsAbstract { get; set; }
        public List<SnatshotRelationType> IncomingRelations { get; set; }
        public List<SnatshotRelationType> OutgoingRelations { get; set; }
    }
    public class SnapshotAttribute
    {
        public Guid Id { get; set; }
        public string Value { get; set; }
    }
    public class SnapshotRelation
    {
        public Guid Id { get; set; }
        public Guid SourceNodeId { get; set; }
        public Guid TargetNodeId { get; set; }
    }
    public class SnapshotNode
    {
        public Guid Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsArchived { get; set; }
        public Guid NodeTypeId { get; set; }
        public SnapshotAttribute Attribute { get; set; }
        public List<SnapshotRelation> IncomingRelations { get; set; }
        public bool IsMigrated { get; set; } = false;
    }
}
