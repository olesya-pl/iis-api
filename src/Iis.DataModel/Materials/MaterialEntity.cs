using System;
using System.Collections.Generic;
using Iis.Interfaces.Materials;

namespace Iis.DataModel.Materials
{
    public class MaterialEntity : BaseEntity, IMaterialEntity
    {
        public Guid? ParentId { get; set; }
        public virtual MaterialEntity Parent { get; set; }

        public Guid? FileId { get; set; }
        public virtual FileEntity File { get; set; }

        public string Metadata { get; set; }
        public string Data { get; set; }
        public string Type { get; set; }
        public string Source { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public Guid? ImportanceSignId { get; set; }
        public Guid? ReliabilitySignId { get; set; }
        public Guid? RelevanceSignId { get; set; }
        public Guid? CompletenessSignId { get; set; }
        public Guid? SourceReliabilitySignId { get; set; }

        public MaterialSignEntity Importance { get; set; }
        public MaterialSignEntity Reliability { get; set; }
        public MaterialSignEntity Relevance { get; set; }
        public MaterialSignEntity Completeness { get; set; }
        public MaterialSignEntity SourceReliability { get; set; }
        public string Title { get; set; }
        public string LoadData { get; set; }

        public virtual ICollection<MaterialEntity> Children { get; set; }
        public virtual ICollection<MaterialInfoEntity> MaterialInfos { get; set; }
    }
}
