using System;
using System.Collections.Generic;

namespace IIS.Core.Ontology.EntityFramework.Context
{
    public class Node
    {
        public Guid Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsArchived { get; set; }
        public Guid TypeId { get; set; }

        public virtual Type Type { get; set; }
        public virtual ICollection<Relation> IncomingRelations { get; set; } = new List<Relation>();
        public virtual ICollection<Relation> OutgoingRelations { get; set; } = new List<Relation>();
    }

    public class Relation
    {
        public Guid Id { get; set; }
        public Guid SourceNodeId { get; set; }
        public Guid TargetNodeId { get; set; }

        public virtual Node Node { get; set; }
        public virtual Node SourceNode { get; set; }
        public virtual Node TargetNode { get; set; }
    }

    public class Attribute
    {
        public Guid Id { get; set; }
        public string Value { get; set; }

        public virtual Node Node { get; set; }
    }
}
