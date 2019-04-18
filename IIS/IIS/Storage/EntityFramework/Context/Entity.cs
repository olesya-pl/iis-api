using System;
using System.Collections.Generic;

namespace IIS.Storage.EntityFramework.Context
{
    public partial class Entity
    {
        public Entity()
        {
            EntityAttributeValues = new HashSet<AttributeValue>();
            EntityRelationsInitiator = new HashSet<Relation>();
            EntityRelationsTarget = new HashSet<Relation>();
        }

        public long Id { get; set; }
        public int TypeId { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }

        public virtual EntityType Type { get; set; }
        public virtual ICollection<AttributeValue> EntityAttributeValues { get; set; }
        public virtual ICollection<Relation> EntityRelationsInitiator { get; set; }
        public virtual ICollection<Relation> EntityRelationsTarget { get; set; }
    }
}
