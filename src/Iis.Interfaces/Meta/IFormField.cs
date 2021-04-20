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
        string Type { get; set; }
        string Hint { get; set; }
        string Icon { get; set; }
        int? Lines { get; set; }

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