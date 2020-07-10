using Iis.Interfaces.Meta;

namespace Iis.OntologySchema.DataTypes
{
    public interface ISchemaMeta: IMeta
    {
        bool? ExposeOnApi { get; set; }
        bool? HasFewEntities { get; set; }
        int? SortOrder { get; set; }
    }
}