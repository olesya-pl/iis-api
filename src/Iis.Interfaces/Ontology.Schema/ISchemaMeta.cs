﻿using Iis.Interfaces.Meta;

namespace Iis.OntologySchema.DataTypes
{
    public interface ISchemaMeta: IMeta
    {
        bool? ExposeOnApi { get; set; }
        bool? HasFewEntities { get; set; }
        int? SortOrder { get; set; }
        string Title { get; set; }
        IFormField FormField { get; set; }
        IContainerMeta Container { get; set; }
        bool Multiple { get; set; }
        IValidation Validation { get; set; }
        IInversedRelationMeta Inversed { get; set; }
    }
}