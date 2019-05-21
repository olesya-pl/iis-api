using System;
using System.Collections.Generic;

namespace IIS.Ontology.EntityFramework
{
    public partial class OAttributeValue
    {
        public long Id { get; set; }
        public long EntityId { get; set; }
        public int AttributeId { get; set; }
        public string Value { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }

        public virtual OAttribute Attribute { get; set; }
        public virtual OEntity Entity { get; set; }
    }
}
