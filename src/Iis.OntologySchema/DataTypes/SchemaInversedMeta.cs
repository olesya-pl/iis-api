using Iis.Interfaces.Meta;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.OntologySchema.DataTypes
{
    public class SchemaInversedMeta : IInversedRelationMeta
    {
        public string Code { get; set; }
        public bool Editable { get; set; }
        public int? SortOrder { get; set; }
        public string Title { get; set; }
        public IFormField FormField { get; set; }
        public IContainerMeta Container { get; set; }
        public bool Multiple { get; set; }
        public IValidation Validation { get; set; }
    }
}
