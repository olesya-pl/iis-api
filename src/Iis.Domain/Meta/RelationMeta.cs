using Iis.Interfaces.Meta;
using System;

namespace Iis.Domain.Meta
{

    public class RelationMetaBase: IRelationMetaBase, IMeta
    {
        public int? SortOrder { get; set; }
        public string Title { get; set; }
        public FormField FormField { get; set; }
        public ContainerMeta Container { get; set; }
        public bool Multiple { get; set; }
        public Validation Validation { get; set; }
        IFormField IRelationMetaBase.FormField => FormField;
        IContainerMeta IRelationMetaBase.Container => Container;
        IValidation IRelationMetaBase.Validation => Validation;
    }

    // Entity to entity relation
    public class EntityRelationMeta : RelationMetaBase, IEntityRelationMeta
    {
        public EntityOperation[] AcceptsEntityOperations { get; set; } // remake to flags
        public string Type { get; set; }
        public InversedRelationMeta Inversed { get; set; }
        public string[] TargetTypes { get; set; }
        IInversedRelationMeta IEntityRelationMeta.Inversed => Inversed;
    }

    public class AttributeRelationMeta : RelationMetaBase
    {
        public string Formula { get; set; }
        public string Format { get; set; }
    }

    // Describes virtual inversed relation
    public class InversedRelationMeta : RelationMetaBase, IInversedRelationMeta
    {
        public string Code { get; set; }
        public bool Editable { get; set; }
    }

    // TODO: this should be an anonymous object, something like JObject
    //       different clients will have different UI components and their configuration cannot be statically described in code!
    public class FormField : IFormField
    {
        public string Type { get; set; }
        public int? Lines { get; set; }
        public string Hint { get; set; }
        [Obsolete]
        public bool? HasIndexColumn { get; set; }
        [Obsolete]
        public bool? IncludeParent { get; set; }
        [Obsolete]
        public string RadioType { get; set; }
        public string Layout { get; set; }
        public string Icon { get; set; }
    }
}
