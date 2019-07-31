using System;

namespace IIS.Core.Ontology.Meta
{
    public interface ITypeMeta
    {
        IValidation Validation { get; set; }
        FormField FormField { get; set; }
        SearchType? Kind { get; set; }
        string Format { get; set; }
        string Formula { get; set; }
        int? SortOrder { get; set; }
        bool? ExposeOnApi { get; set; }
    }
    
    public interface IValidation
    {
        bool? Required { get; set; }
    }

    public class TypeMeta : ITypeMeta
    {
        public IValidation Validation { get; set; }
        public FormField FormField { get; set; }
        public SearchType? Kind { get; set; }
        public string Format { get; set; }
        public string Formula { get; set; }
        // ----- added ----- //
        public int? SortOrder { get; set; }
        public bool? ExposeOnApi { get; set; }
        public bool? HasFewEntities { get; set; } // Ignore and remove
    }

    public class Validation : IValidation
    {
        public bool? Required { get; set; }
    }

    public class StringValidation : Validation
    {
        public int? MinLength { get; set; }
        public int? MaxLength { get; set; }
        public bool? Email { get; set; } // think about some kind of dynamic string validators
        public bool? Url { get; set; }
        public string Pattern { get; set; }
    }

    public class IntValidation : Validation
    {
        public int? Min { get; set; }
        public int? Max { get; set; }
    }

    public class DateValidation : Validation
    {
        public DateTime Min { get; set; }
        public DateTime Max { get; set; }
    }

    public class FormField
    {
        public string Type { get; set; }
        public int? Lines { get; set; }
        public string Hint { get; set; }
    }

    
    // On relation type - connect inversed relation with direct
    class MetaForEntityTypeRelation : TypeMeta
    {
        public InversedEntityTypeRelation Inversed { get; set; }
        public bool Multiple { get; set; } // why are you here?
    }

    class InversedEntityTypeRelation
    {
        public string Code { get; set; }
        public bool Multiple { get; set; } // why are you here?
        public bool Editable { get; set; } // why are you here?
    }

    public class RelationTypeMeta : TypeMeta
    {
        public string Title { get; set; }
        public bool? Multiple { get; set; }
        public int? Cardinality { get; set; } // what is it?
        public bool? Inversed { get; set; }
        public EntityOperation[] AcceptsEntityOperations { get; set; } // remake to flags
        // validation already present on TypeMeta
        public string Type { get; set; }
    }

    // On relation
    public class InversedRelationTypeMeta // : TypeMeta ?
    {
        public int? Cardinality { get; set; }
        // multiple
        // editable
    }

    public enum RelationStructureType
    {
        Graph, Tree
    }

    public enum SearchType
    {
        Keyword
    }

    public enum EntityOperation
    {
        Create, Update, Delete
    }
}