using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.OntologySchema.Comparison
{
    public class SchemaCompareDiffItem
    {
        public string NodeStringCode { get; private set; }
        public string OldValue { get; private set; }
        public string NewValue { get; private set; }
        public SchemaCompareDiffKind DiffKind { get; private set; }
        public SchemaCompareDiffItem(string nodeStringCode, string oldValue, string newValue, SchemaCompareDiffKind diffKind)
        {
            NodeStringCode = nodeStringCode;
            OldValue = oldValue;
            NewValue = newValue;
            DiffKind = diffKind;
        }
    }
}
