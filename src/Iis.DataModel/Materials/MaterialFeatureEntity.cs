using System;

namespace Iis.DataModel.Materials
{
    public class MaterialFeatureEntity : BaseEntity
    {
        public Guid MaterialInfoId { get; set; }
        public virtual MaterialInfoEntity MaterialInfo { get; set; }

        public Guid NodeId { get; set; }
        public virtual NodeEntity Node { get; set; }
        public NodeEntityType NodeType { get; set; }

        public string Relation { get; set; }
        public string Value { get; set; }
    }

    public enum NodeEntityType
    {
        Entity = 0,
        Event = 1,
        Feature = 2
    }
}
