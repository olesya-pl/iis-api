using System;
using System.Collections.Generic;

namespace IIS.Storage.EntityFramework.Context
{
    public partial class OEntity
    {
        public OEntity()
        {
            AttributeValues = new HashSet<OAttributeValue>();
            ForwardRelations = new HashSet<ORelation>();
            BackwardRelations = new HashSet<ORelation>();
        }

        public long Id { get; set; }
        public int TypeId { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }

        public virtual OType Type { get; set; }
        public virtual ICollection<OAttributeValue> AttributeValues { get; set; }
        public virtual ICollection<ORelation> ForwardRelations { get; set; }
        public virtual ICollection<ORelation> BackwardRelations { get; set; }
    }
}
