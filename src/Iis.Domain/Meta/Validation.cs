using Iis.Interfaces.Meta;
using System;

namespace Iis.Domain.Meta
{
    public class Validation : IValidation
    {
        public bool? Required { get; set; } // Remove from meta to schema
    }

    public class StringValidation : Validation
    {
        public int? MinLength { get; set; }
        public int? MaxLength { get; set; }
        public bool? Email { get; set; } // think about some kind of dynamic string validators
        public bool? Url { get; set; }
        public string Pattern { get; set; }
        public bool? NotEmpty { get; set; } // to be implemented
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
