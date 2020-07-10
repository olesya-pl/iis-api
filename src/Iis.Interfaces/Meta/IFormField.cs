using System;

namespace Iis.Interfaces.Meta
{
    public interface IFormField
    {
        [Obsolete]
        bool? HasIndexColumn { get; set; }
        string Hint { get; set; }
        string Icon { get; set; }
        [Obsolete]
        bool? IncludeParent { get; set; }
        string Layout { get; set; }
        int? Lines { get; set; }
        [Obsolete]
        string RadioType { get; set; }
        string Type { get; set; }
    }
}