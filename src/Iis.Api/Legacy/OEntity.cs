﻿using System;
using System.Collections.Generic;

namespace IIS.Legacy.EntityFramework
{
    public partial class OEntity
    {
        public OEntity()
        {
            AttributeValues = new HashSet<OAttributeValue>();
            ForwardRelations = new HashSet<ORelation>();
            BackwardRelations = new HashSet<ORelation>();
        }

        public Guid Id { get; set; }
        public Guid TypeId { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }

        public virtual OTypeEntity Type { get; set; }
        public virtual ICollection<OAttributeValue> AttributeValues { get; set; }
        public virtual ICollection<ORelation> ForwardRelations { get; set; }
        public virtual ICollection<ORelation> BackwardRelations { get; set; }
    }
}