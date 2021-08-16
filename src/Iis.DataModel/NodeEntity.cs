using System;
using System.Collections.Generic;
using Iis.DataModel.Materials;
using Iis.DataModel.Reports;
using Iis.Interfaces.Ontology.Data;

namespace Iis.DataModel
{
    public class NodeEntity : BaseEntity, INodeBase, IAuditable
    {
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsArchived { get; set; }

        public Guid NodeTypeId { get; set; }
        public virtual NodeTypeEntity NodeType { get; set; }

        public virtual ICollection<RelationEntity> IncomingRelations { get; set; } = new List<RelationEntity>();
        public virtual ICollection<RelationEntity> OutgoingRelations { get; set; } = new List<RelationEntity>();

        public virtual AttributeEntity Attribute { get; set; }
        public virtual RelationEntity Relation { get; set; }

        public List<MaterialFeatureEntity> MaterialFeatures { get; set; }
        public ICollection<ReportEventEntity> ReportEvents { get; internal set; }
    }
}
