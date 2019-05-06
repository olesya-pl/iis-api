using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace IIS.Storage.EntityFramework.Context
{
    public partial class ORestriction
    {
        public int Id { get; set; }
        public int RelationTypeId { get; set; }
        public int SourceId { get; set; }
        public int TargetId { get; set; }
        public JObject Meta { get; set; }

        public virtual OTypeEntity Source { get; set; }
        public virtual OTypeRelation RelationType { get; set; }
        public virtual OTypeEntity Target { get; set; }

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
                return meta != null && meta.Value<bool>();
            }
        }
    }
}
