using System;
using System.Collections.Generic;

namespace IIS.OSchema.EntityFramework
{
    public partial class ORelation
    {
        public long Id { get; set; }
        public int TypeId { get; set; }
        public long SourceId { get; set; }
        public long TargetId { get; set; }
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
