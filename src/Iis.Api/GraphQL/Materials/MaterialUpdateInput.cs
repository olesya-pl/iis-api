using System;
using System.Collections.Generic;

using Iis.Interfaces.Materials;

namespace IIS.Core.GraphQL.Materials
{
    public class MaterialUpdateInput : IMaterialUpdateInput
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Content {get;set;}
        public Guid? ImportanceId { get; set; }
        public Guid? ReliabilityId { get; set; }
        public Guid? RelevanceId { get; set; }
        public Guid? CompletenessId { get; set; }
        public Guid? SourceReliabilityId { get; set; }
        public Guid? ProcessedStatusId { get; set; }
        public Guid? SessionPriorityId { get; set; }
        public Guid? AssigneeId { get; set; }
        public IEnumerable<string> Objects { get; set; }
        public IEnumerable<string> Tags { get; set; }
        public IEnumerable<string> States { get; set; }
    }
}
