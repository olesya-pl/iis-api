using System;
using System.Collections.Generic;

namespace Iis.DataModel.Materials
{
    public class MaterialEntity : BaseEntity
    {
        public static readonly Guid ProcessingStatusSignTypeId
            = new Guid("214ceeee-67d5-4692-a3b4-316007fa5d34");

        public static readonly Guid ProcessingStatusProcessedSignId
            = new Guid("c85a76f4-3c04-46f7-aed9-f865243b058e");

        public static readonly Guid ProcessingStatusNotProcessedSignId
            = new Guid("0a641312-abb7-4b40-a766-0781308eb077");

        public Guid? ParentId { get; set; }
        public virtual MaterialEntity Parent { get; set; }
        public Guid? FileId { get; set; }
        public virtual FileEntity File { get; set; }
        public string Metadata { get; set; }
        public string Data { get; set; }
        public string Type { get; set; }
        public string Source { get; set; }
        public string Content { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public Guid? ImportanceSignId { get; set; }
        public Guid? ReliabilitySignId { get; set; }
        public Guid? RelevanceSignId { get; set; }
        public Guid? CompletenessSignId { get; set; }
        public Guid? SourceReliabilitySignId { get; set; }
        public Guid? SessionPriorityId { get; set; }
        public MaterialSignEntity Importance { get; set; }
        public MaterialSignEntity Reliability { get; set; }
        public MaterialSignEntity Relevance { get; set; }
        public MaterialSignEntity Completeness { get; set; }
        public MaterialSignEntity SourceReliability { get; set; }
        public MaterialSignEntity SessionPriority { get; set; }
        public string Title { get; set; }
        public string LoadData { get; set; }
        public Guid? ProcessedStatusSignId { get; set; }
        public MaterialSignEntity ProcessedStatus { get; set; }
        public virtual ICollection<MaterialEntity> Children { get; set; }
        public virtual ICollection<MaterialInfoEntity> MaterialInfos { get; set; }
        public Guid? AssigneeId { get; set; }
        public virtual UserEntity Assignee { get; set; }
        public int MlHadnlersCount { get; set; }
    }
}
