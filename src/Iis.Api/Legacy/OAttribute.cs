using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Iis.Utility;

namespace IIS.Legacy.EntityFramework
{
    public partial class OAttribute
    {
        public OAttribute()
        {
            Values = new HashSet<OAttributeValue>();
            Restrictions = new HashSet<OAttributeRestriction>();
        }

        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Code { get; set; }
        public JObject Meta { get; set; }
        public ScalarType Type { get; set; }

        public virtual ICollection<OAttributeValue> Values { get; set; }
        public virtual ICollection<OAttributeRestriction> Restrictions { get; set; }

        public bool IsRequired
        {
            get
            {
                var meta = Meta["validation"]?["required"];
                return meta != null && meta.Value<bool>();
            }
        }

        public ScalarType Kind
        {
            get
            {
                var meta = Meta["kind"];
                return meta == null ? Type : (ScalarType)meta.Value<string>();
            }
        }
    }
}
