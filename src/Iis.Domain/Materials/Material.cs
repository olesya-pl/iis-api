using System;
using System.Collections.Generic;
using Iis.Roles;
using Newtonsoft.Json.Linq;

namespace Iis.Domain.Materials
{
    public class Material
    {
        public Guid Id { get; set; }
        public Guid? ParentId { get; set; }
        public JArray Data { get; set; }
        public JObject Metadata { get; set; }
        public string Type { get; set; }
        public string Source { get; set; }
        public string Content { get; set; }
        public MaterialSign Importance { get; set; }
        public Guid? ImportanceSignId => Importance?.Id;
        public MaterialSign Reliability { get; set; }
        public Guid? ReliabilitySignId => Reliability?.Id;
        public MaterialSign Relevance { get; set; }
        public Guid? RelevanceSignId => Relevance?.Id;
        public MaterialSign Completeness { get; set; }
        public Guid? CompletenessSignId => Completeness?.Id;
        public MaterialSign SourceReliability { get; set; }
        public Guid? SourceReliabilitySignId => SourceReliability?.Id;
        public MaterialSign ProcessedStatus { get; set; }
        public Guid? ProcessedStatusSignId => ProcessedStatus?.Id;
        public MaterialSign SessionPriority { get; set; }
        public Guid? SessionPriorityId => SessionPriority?.Id;
        public List<Material> Children { get; } = new List<Material>();
        public FileInfo File { get; set; }
        public Guid? FileId => File?.Id;
        public List<MaterialInfo> Infos { get; } = new List<MaterialInfo>();
        public DateTime CreatedDate { get; set; }
        public string Title { get; set; }
        public MaterialLoadData LoadData { get; set; }
        public User Assignee { get; set; }
        public Guid? AssigneeId { get; set; }
        public int MlHandlersCount { get; set; }
        public int ProcessedMlHandlersCount { get; set; }
        public IEnumerable<MaterialRelation> ObjectsOfStudy { get; set; }
        public IEnumerable<Node> Events { get; set; }
        public IEnumerable<JObject> Features { get; set; }
    }
}
