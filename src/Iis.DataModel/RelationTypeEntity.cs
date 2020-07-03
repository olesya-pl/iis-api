using Iis.Interfaces.Ontology.Schema;
using System;

namespace Iis.DataModel
{
    public class RelationTypeEntity : BaseEntity, IRelationType
    {
        public virtual NodeTypeEntity INodeTypeModel { get; set; }

        public RelationKind Kind { get; set; }
        public EmbeddingOptions EmbeddingOptions { get; set; }

        public Guid SourceTypeId { get; set; }
        public virtual NodeTypeEntity SourceType { get; set; }

        public Guid TargetTypeId { get; set; }
        public virtual NodeTypeEntity TargetType { get; set; }
    }
}
