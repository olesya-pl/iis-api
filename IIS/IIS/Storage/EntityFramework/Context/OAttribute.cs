using System;
using System.Collections.Generic;

namespace IIS.Storage.EntityFramework.Context
{
    public partial class OAttribute
    {
        public OAttribute()
        {
            Values = new HashSet<OAttributeValue>();
            Restrictions = new HashSet<OAttributeRestriction>();
        }

        public int Id { get; set; }
        public string Title { get; set; }
        public string Code { get; set; }
        public string Meta { get; set; }
        public string Type { get; set; }

        public virtual ICollection<OAttributeValue> Values { get; set; }
        public virtual ICollection<OAttributeRestriction> Restrictions { get; set; }

        public bool IsRequired { get => Meta.Contains("\"required\": true"); }
    }
}
