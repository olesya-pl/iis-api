using Iis.Interfaces.Materials;

namespace Iis.Api.GraphQL.Materials
{
    public static class MaterialUpdateInputExtensions
    {
        public static bool HasValue(this IMaterialUpdateInput materialUpdateInput)
        {
            return materialUpdateInput.Title != null
            || materialUpdateInput.Content != null
            || materialUpdateInput.ImportanceId != null
            || materialUpdateInput.ReliabilityId != null
            || materialUpdateInput.RelevanceId != null
            || materialUpdateInput.CompletenessId != null
            || materialUpdateInput.SourceReliabilityId != null
            || materialUpdateInput.ProcessedStatusId != null
            || materialUpdateInput.SessionPriorityId != null
            || materialUpdateInput.AssigneeIds != null
            || materialUpdateInput.Objects != null
            || materialUpdateInput.Tags != null
            || materialUpdateInput.States != null;
        }
    }
}