using System;

namespace Iis.Interfaces.Meta
{
    public enum FormFieldTypes
    {
        dropdown,
        dropdownTree,
        form,
        fuzzyDate,
        fuzzyDateRange,
        radioGroup,
        taggableString,
    };
    public interface IFormField
    {
        int? Lines { get; set; }

        [Obsolete("Changed to computed property in Attributes.cs")]
        string Type { get; set; }
        [Obsolete]
        string Hint { get; set; }
        [Obsolete("Changed to computed property in Attributes.cs")]
        string Icon { get; set; }
        [Obsolete]
        bool? HasIndexColumn { get; set; }
        [Obsolete]
        bool? IncludeParent { get; set; }
        [Obsolete]
        string Layout { get; set; }
        [Obsolete]
        string RadioType { get; set; }
    }
}