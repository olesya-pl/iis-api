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
    }
}
