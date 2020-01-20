using System;

namespace Iis.DataModel
{
    public class RelationEntity : BaseEntity
    {
        public virtual NodeEntity Node { get; set; }

        public Guid SourceNodeId { get; set; }
        public virtual NodeEntity SourceNode { get; set; }

        public Guid TargetNodeId { get; set; }
        public virtual NodeEntity TargetNode { get; set; }
    }
}
