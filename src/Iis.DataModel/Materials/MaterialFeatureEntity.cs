using System;
using Iis.Interfaces.Ontology.Schema;

namespace Iis.DataModel.Materials
{
    public class MaterialFeatureEntity : BaseEntity
    {
        public Guid MaterialInfoId { get; set; }
        public virtual MaterialInfoEntity MaterialInfo { get; set; }

        public Guid NodeId { get; set; }
        public virtual NodeEntity Node { get; set; }
        public EntityTypeNames NodeType { get; set; }

        public string Relation { get; set; }
        public string Value { get; set; }
    }
}
