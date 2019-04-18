using System;
using System.Collections.Generic;

namespace IIS.Storage.EntityFramework.Context
{
    public partial class EntityAttribute
    {
        public EntityAttribute()
        {
            EntityAttributeValues = new HashSet<AttributeValue>();
            EntityTypeAttributes = new HashSet<EntityTypeAttribute>();
        }

        public int Id { get; set; }
        public string Title { get; set; }
        public string Code { get; set; }
        public string Meta { get; set; }
        public string Type { get; set; }

        public virtual ICollection<AttributeValue> EntityAttributeValues { get; set; }
        public virtual ICollection<EntityTypeAttribute> EntityTypeAttributes { get; set; }
    }
}
