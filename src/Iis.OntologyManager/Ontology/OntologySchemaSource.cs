using Iis.Interfaces.Ontology.Schema;
using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.OntologyManager.Ontology
{
    public class OntologySchemaSource : IOntologySchemaSource
    {
        public string Title { get; set; }
        public SchemaSourceKind SourceKind { get; set; }
        public string Data { get; set; }
        public override string ToString()
        {
            return Title;
        }
    }
}
