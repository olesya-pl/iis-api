using System;
using System.Collections.Generic;

namespace IIS.Core.Ontology.EntityFramework.Context
{
    public class Type
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

        public virtual ICollection<RelationType> IncomingRelations { get; set; } = new List<RelationType>();
        public virtual ICollection<RelationType> OutgoingRelations { get; set; } = new List<RelationType>();
        public virtual ICollection<Node> Nodes { get; set; } = new List<Node>();

        public virtual AttributeType AttributeType { get; set; }
        public virtual RelationType RelationType { get; set; }
    }

    public enum Kind { Entity, Relation, Attribute }
}
