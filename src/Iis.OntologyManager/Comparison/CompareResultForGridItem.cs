using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.OntologyManager.Comparison
{
    public enum CompareResultOperation
    {
        Insert,
        Update,
        Delete
    };

    public enum CompareResultItemType
    {
        NodeType,
        Alias
    };

    public class CompareResultForGridItem
    {
        public bool Checked { get; set; }
        public string Title { get; set; }
        public string NewValue { get; set; }
        public string OldValue { get; set; }
        public CompareResultOperation Operation { get; set; }
        public CompareResultItemType ItemType { get; set; }
        public object LinkedObject { get; set; }
    }
}
