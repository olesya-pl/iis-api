using System;

namespace IIS.Core.Ontology.EntityFramework.Context
{
    public class RelationType
    {
        public Guid Id { get; set; }
        public RelationKind Kind { get; set; }
        public Guid SourceTypeId { get; set; }
        public Guid TargetTypeId { get; set; }

        public virtual Type Type { get; set; }
        public virtual Type SourceType { get; set; }
        public virtual Type TargetType { get; set; }
    }

    public enum RelationKind { Embedding, Inheritance }
}
