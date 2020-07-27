using Iis.Interfaces.Meta;
using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.OntologySchema.DataTypes
{
    public class SchemaValidation: IValidation
    {
        public bool? Required { get; set; }
    }
}
