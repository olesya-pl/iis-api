using System;
using System.Collections.Generic;

namespace IIS.Storage.EntityFramework.Context
{
    public partial class ORelation
    {
        public long Id { get; set; }
        public int TypeId { get; set; }
        public long InitiatorId { get; set; }
        public long TargetId { get; set; }
        public bool IsInferred { get; set; }
        public DateTime? StartsAt { get; set; }
        public DateTime? EndsAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }

        public virtual OEntity Source { get; set; }
        public virtual OEntity Target { get; set; }
        public virtual OType Type { get; set; }
    }
}
