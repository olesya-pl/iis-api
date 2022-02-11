using System;
using System.Collections.Generic;
using Iis.Domain.Users;
using Iis.Interfaces.Common;
using Iis.Interfaces.SecurityLevels;
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
        public string Channel { get; set; }
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
        public List<Material> Children { get; set; } = new List<Material>();
        public File File { get; set; }
        public Guid? FileId => File?.Id;
        public List<MaterialInfo> Infos { get; } = new List<MaterialInfo>();
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string Title { get; set; }
        public MaterialLoadData LoadData { get; set; }
        public IReadOnlyCollection<User> Assignees { get; set; } = Array.Empty<User>();
        public User Editor { get; set; }
        public Guid? EditorId { get; set; }
        public int MlHandlersCount { get; set; }
        public int ProcessedMlHandlersCount { get; set; }
        public JObject ObjectsOfStudy { get; set; }
        public IEnumerable<JObject> Events { get; set; }
        public IEnumerable<JObject> Features { get; set; }
        public bool CanBeEdited { get; set; }
        public int AccessLevel { get; set; }
        public DateTime? RegistrationDate { get; set; }
        public bool HasAttachedFile() => File != null;
        public bool IsParentMaterial() => ParentId == null;
        public SubscriberDto Caller { get; set; }
        public SubscriberDto Receiver { get; set; }
        public IReadOnlyCollection<RelatedObjectOfStudy> RelatedObjectCollection { get; set; } = Array.Empty<RelatedObjectOfStudy>();
        public IReadOnlyCollection<RelatedObject> RelatedEventCollection { get; set; } = Array.Empty<RelatedObject>();
        public IReadOnlyCollection<RelatedObject> RelatedSignCollection { get; set; } = Array.Empty<RelatedObject>();
        public int ObjectsOfStudyCount { get; set; }
        public IReadOnlyCollection<ISecurityLevel> SecurityLevels { get; set; }
        public bool AccessAllowed { get; set; }
    }
}