using Iis.Interfaces.Meta;
using System;

namespace Iis.OntologySchema.DataTypes
{
    public interface ISchemaMeta
    {
        #region Common Fields

        public bool? Hidden { get; }
        string Formula { get; }
        bool? IsAggregated { get; }

        #endregion

        #region Front-End Fields

        int? SortOrder { get; }
        string Format { get; }
        public bool? IsImportantRelation { get; }
        EntityOperation[] AcceptsEntityOperations { get; }
        string[] TargetTypes { get; }
        IFormField FormField { get; }
        IContainerMeta Container { get; }
        IInversedMeta Inversed { get; }

        #endregion

        #region Obsolete Fields

        [Obsolete]
        IValidation Validation { get; }
        
        #endregion
    }
}