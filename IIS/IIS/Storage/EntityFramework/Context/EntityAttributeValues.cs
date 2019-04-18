using System;
using System.Collections.Generic;

namespace IIS.Storage.EntityFramework.Context
{
    public partial class EntityAttributeValues
    {
        public long Id { get; set; }
        public long EntityId { get; set; }
        public int AttributeId { get; set; }
        public string Value { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}
