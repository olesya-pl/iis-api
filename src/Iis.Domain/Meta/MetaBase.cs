using Iis.Interfaces.Meta;
using Iis.OntologySchema.DataTypes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.Domain.Meta
{
    public class MetaBase: ISchemaMeta
    {
        public string Title { get; set; }
        public int? SortOrder { get; set; }
        public bool? ExposeOnApi { get; set; }
        public bool? HasFewEntities { get; set; }
        public EntityOperation[] AcceptsEmbeddedOperations { get; set; }
        public FormField FormField { get; set; }
        public ContainerMeta Container { get; set; }
        public bool Multiple { get; set; }
        public Validation Validation { get; set; }
        public EntityOperation[] AcceptsEntityOperations { get; set; }
        public string Type { get; set; }
        public InversedRelationMeta Inversed { get; set; }
        public string[] TargetTypes { get; set; }
        public string Code { get; set; }
        public bool Editable { get; set; }
        public string Formula { get; set; }
        public string Format { get; set; }
        public bool? IsAggregated { get; set; }

        IFormField ISchemaMeta.FormField => FormField;
        IContainerMeta ISchemaMeta.Container => Container;
        IValidation ISchemaMeta.Validation => Validation;
        ISchemaMeta ISchemaMeta.Inversed => Inversed;
    }
}
