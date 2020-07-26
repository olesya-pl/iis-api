using Iis.Interfaces.Meta;
using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.OntologySchema.DataTypes
{
    public class SchemaFormField: IFormField
    {
        public string Type { get; set; }
        public int? Lines { get; set; }
        public string Hint { get; set; }
        [Obsolete]
        public bool? HasIndexColumn { get; set; }
        [Obsolete]
        public bool? IncludeParent { get; set; }
        [Obsolete]
        public string RadioType { get; set; }
        public string Layout { get; set; }
        public string Icon { get; set; }
    }
}
