using Iis.Interfaces.Meta;

namespace Iis.OntologySchema.DataTypes
{
    public interface ISchemaMeta
    {
        bool? ExposeOnApi { get; }
        bool? HasFewEntities { get; }
        int? SortOrder { get; }
        string Title { get; }
        string Formula { get; }
        string Format { get; }
        EntityOperation[] AcceptsEntityOperations { get; }
        EntityOperation[] AcceptsEmbeddedOperations { get; }
        string Type { get; }
        string[] TargetTypes { get; }
        IFormField FormField { get; }
        IContainerMeta Container { get; }
        bool Multiple { get; }
        IValidation Validation { get; }
        ISchemaMeta Inversed { get; }
        bool? IsAggregated { get; }
        string Code { get; }
        bool Editable { get; }
    }
}