using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace IIS.Ontology.EntityFramework
{
    public partial class ORestriction
    {
        public Guid Id { get; set; }
        public Guid RelationTypeId { get; set; }
        public Guid SourceId { get; set; }
        public Guid TargetId { get; set; }
        public JObject Meta { get; set; }

        public virtual OTypeEntity Source { get; set; }
        public virtual OTypeRelation Type { get; set; }
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
