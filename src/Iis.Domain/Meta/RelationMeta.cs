using Iis.Interfaces.Meta;
using System;

namespace Iis.Domain.Meta
{

    public class RelationMetaBase: MetaBase
    {
    }

    // Entity to entity relation
    public class EntityRelationMeta : RelationMetaBase
    {
    }

    public class AttributeRelationMeta : RelationMetaBase
    {
    }

    // Describes virtual inversed relation
    public class InversedRelationMeta : RelationMetaBase
    {
    }

    // TODO: this should be an anonymous object, something like JObject
    //       different clients will have different UI components and their configuration cannot be statically described in code!
    public class FormField : IFormField
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
