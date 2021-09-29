using Iis.Interfaces.Meta;
using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.OntologySchema.DataTypes
{
    public class SchemaInversedMeta: IInversedMeta
    {
        public string Code { get; set; }
        public string Title { get; set; }
        public bool Multiple { get; set; }
    }
}
