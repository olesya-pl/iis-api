using System;
using System.Collections.Generic;

namespace IIS.Storage.EntityFramework.Context
{
    public partial class AttributeValue
    {
        public long Id { get; set; }
        public long EntityId { get; set; }
        public int AttributeId { get; set; }
        public string Value { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }

        public virtual EntityAttribute Attribute { get; set; }
        public virtual Entity Entity { get; set; }
    }
}
