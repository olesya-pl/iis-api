namespace Iis.Domain.Meta
{

    public class RelationMetaBase : IMeta
    {
        public int? SortOrder { get; set; }
        public string Title { get; set; }
        public FormField FormField { get; set; }
        public ContainerMeta Container { get; set; }
        public bool Multiple { get; set; }
        public IValidation Validation { get; set; }
    }

    // Entity to entity relation
    public class EntityRelationMeta : RelationMetaBase
    {
        public EntityOperation[] AcceptsEntityOperations { get; set; } // remake to flags
        public string Type { get; set; }
        public InversedRelationMeta Inversed { get; set; }
    }

    public class AttributeRelationMeta : RelationMetaBase
    {
        public string Formula { get; set; }
        public string Format { get; set; }
    }

    // Describes virtual inversed relation
    public class InversedRelationMeta : RelationMetaBase
    {
        public string Code { get; set; }
        public bool Editable { get; set; }
    }

    public enum EntityOperation
    {
        Create, Update, Delete
    }

    // TODO: this should be an anonymous object, something like JObject
    //       different clients will have different UI components and their configuration cannot be statically described in code!
    public class FormField
    {
        public string Type { get; set; }
        public int? Lines { get; set; }
        public string Hint { get; set; }
        public bool? HasIndexColumn { get; set; }
        public bool? IncludeParent { get; set; }
        public string RadioType { get; set; }
        public string Layout { get; set; }
        public string Icon { get; set; }
    }
}
