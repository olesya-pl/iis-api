using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.OntologyManager.Ontology
{
    public enum SchemaSourceKind
    {
        File,
        Database
    }
    public class OntologySchemaSource
    {
        public string Title { get; set; }
        public SchemaSourceKind SourceKind { get; set; }
        public string Data { get; set; }
    }
}
