using System;

namespace IIS.Core.Ontology.Meta
{
    public interface IValidation
    {
        bool? Required { get; set; }
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

    
}