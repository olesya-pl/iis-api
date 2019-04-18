using System;
using System.Collections.Generic;

namespace IIS.Storage.EntityFramework.Context
{
    public partial class EntityTypeAttribute
    {
        public int Id { get; set; }
        public int TypeId { get; set; }
        public int AttributeId { get; set; }
        public string Meta { get; set; }

        public virtual EntityAttribute Attribute { get; set; }
        public virtual EntityType Type { get; set; }
    }
}
