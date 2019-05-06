﻿using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace IIS.Storage.EntityFramework.Context
{
    public partial class OAttributeRestriction
    {
        public int Id { get; set; }
        public int TypeId { get; set; }
        public int AttributeId { get; set; }
        public JObject Meta { get; set; }

        public virtual OAttribute Attribute { get; set; }
        public virtual OType Owner { get; set; }

        public bool IsMultiple
        {
            get
            {
                var meta = Meta["multiple"];
                return meta != null && meta.Value<bool>();
            }
        }

        public bool IsRequired
        {
            get
            {
                var meta = Meta["validation"]?["required"];
                return meta != null && meta.Value<bool>() || Attribute.IsRequired;
            }
        }
    }
}
