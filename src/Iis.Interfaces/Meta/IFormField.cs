using System;

namespace Iis.Interfaces.Meta
{
    public interface IFormField
    {
        bool? HasIndexColumn { get; set; }
        string Hint { get; set; }
        string Icon { get; set; }
        bool? IncludeParent { get; set; }
        string Layout { get; set; }
        int? Lines { get; set; }
        string RadioType { get; set; }
        string Type { get; set; }
    }
}