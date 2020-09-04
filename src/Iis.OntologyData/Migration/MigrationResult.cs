using Iis.Interfaces.Ontology.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.OntologyData.Migration
{
    public class MigrationResult : IMigrationResult
    {
        public string Log { get; set; }
    }
}
