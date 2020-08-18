using Iis.Interfaces.Ontology.Schema;
using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.OntologyManager.Parameters
{
    public class SchemaSaveParameters : ISchemaSaveParameters
    {
        public bool Create { get; set; }

        public bool Update { get; set; }

        public bool Delete { get; set; }

        public bool Aliases { get; set; }
    }
}
