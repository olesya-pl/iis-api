using Iis.Interfaces.Ontology.Schema;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace Iis.DbLayer.OntologySchema
{
    public class OntologySchemaSource : IOntologySchemaSource
    {
        public OntologySchemaSource(IConfiguration configuration)
        {
            Data = configuration.GetConnectionString("db");
        }
        public OntologySchemaSource() { }
        public string Title { get; set; }
        public SchemaSourceKind SourceKind { get; set; }
        public string Data { get; set; }
        public override string ToString()
        {
            return Title;
        }
    }
}
