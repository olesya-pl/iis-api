﻿using System;
using System.Collections.Generic;

namespace Iis.Interfaces.Materials
{
    public interface IMaterialUpdateInput
    {
        Guid? CompletenessId { get; set; }
        Guid Id { get; set; }
        Guid? ImportanceId { get; set; }
        IEnumerable<string> Objects { get; set; }
        Guid? RelevanceId { get; set; }
        Guid? ReliabilityId { get; set; }
        Guid? SourceReliabilityId { get; set; }
        Guid? ProcessedStatusId { get; set; }
        Guid? SessionPriorityId { get; set; }
        IReadOnlyCollection<Guid> AssigneeIds { get; set; }
        IEnumerable<string> States { get; set; }
        IEnumerable<string> Tags { get; set; }
        string Title { get; set; }
        string Content { get; set; }
        IReadOnlyList<Guid> SecurityLevels { get; set; }
    }
}