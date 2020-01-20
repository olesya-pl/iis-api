using System;
using System.Collections.Generic;

namespace IIS.Legacy.EntityFramework
{
    public partial class OAttributeValue
    {
        public Guid Id { get; set; }
        public Guid EntityId { get; set; }
        public Guid AttributeId { get; set; }
        public string Value { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }

        public virtual OAttribute Attribute { get; set; }
        public virtual OEntity Entity { get; set; }
    }
}
