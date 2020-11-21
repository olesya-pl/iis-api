using Iis.Interfaces.Meta;
using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.Domain.Meta
{
    public class MetaBase: IEntityMeta, IAttributeMeta, IAttributeRelationMeta, IEntityRelationMeta, IInversedRelationMeta
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

        IFormField IMeta.FormField => FormField;
        IContainerMeta IMeta.Container => Container;
        IValidation IMeta.Validation => Validation;
        IInversedRelationMeta IMeta.Inversed => Inversed;
    }
}
