using System;

namespace Iis.DataModel.Materials
{
    public enum MaterialNodeLinkType
    {
        None = 0,
        Caller = 1,
        Receiver = 2
    }
    public class MaterialFeatureEntity : BaseEntity
    {
        public Guid MaterialInfoId { get; set; }
        public virtual MaterialInfoEntity MaterialInfo { get; set; }

        public Guid NodeId { get; set; }
        public virtual NodeEntity Node { get; set; }

        public string Relation { get; set; }
        public string Value { get; set; }
        public MaterialNodeLinkType NodeLinkType { get; set; }
    }
}
