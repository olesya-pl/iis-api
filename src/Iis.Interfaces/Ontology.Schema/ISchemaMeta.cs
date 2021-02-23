using Iis.Interfaces.Meta;
using System;

namespace Iis.OntologySchema.DataTypes
{
    public interface ISchemaMeta
    {
        [Obsolete]
        bool? ExposeOnApi { get; }
        [Obsolete]
        bool? HasFewEntities { get; }
        int? SortOrder { get; }
        string Title { get; }
        string Formula { get; }
        string Format { get; }
        EntityOperation[] AcceptsEntityOperations { get; }
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
        public bool? IsImportantRelation { get; }
        public bool Disabled { get; }
    }
}