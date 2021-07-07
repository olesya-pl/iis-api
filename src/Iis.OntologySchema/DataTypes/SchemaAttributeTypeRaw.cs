using Iis.Interfaces.Ontology.Schema;
using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.OntologySchema.DataTypes
{
    public class SchemaAttributeTypeRaw : IAttributeType
    {
        public Guid Id { get; set; }
        public ScalarType ScalarType { get; set; }
        public string GetDefaultValue()
        {

            return ScalarType switch
            {
                ScalarType.String => "",
                ScalarType.Int => "0",
                ScalarType.Decimal => "0",
                ScalarType.Date => "1970-01-01T00:00:00.0000000Z",
                ScalarType.Boolean => "false",
                ScalarType.Geo => "{\"type\":\"Point\",\"coordinates\":[0,0]}",
                _ => ""
            };
        }
    }
}
