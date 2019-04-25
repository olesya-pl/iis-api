using System;
using System.Collections.Generic;

namespace IIS.Storage.EntityFramework.Context
{
    public partial class OAttributeRestriction
    {
        public int Id { get; set; }
        public int TypeId { get; set; }
        public int AttributeId { get; set; }
        public string Meta { get; set; }

        public virtual OAttribute Attribute { get; set; }
        public virtual OType Type { get; set; }

        // todo: create field
        public bool IsMultiple { get => Meta.Contains("\"multiple\": true"); }
        public bool IsRequired { get => Meta.Contains("\"required\": true") || Attribute.IsRequired; }
    }
}
