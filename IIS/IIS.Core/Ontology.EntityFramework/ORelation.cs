using System;
using System.Collections.Generic;

namespace IIS.Ontology.EntityFramework
{
    public partial class ORelation
    {
        public Guid Id { get; set; }
        public Guid TypeId { get; set; }
        public Guid SourceId { get; set; }
        public Guid TargetId { get; set; }
        public bool IsInferred { get; set; }
        public DateTime? StartsAt { get; set; }
        public DateTime? EndsAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }

        public virtual OEntity Source { get; set; }
        public virtual OEntity Target { get; set; }
        public virtual OTypeRelation Type { get; set; }
    }
}
