using Iis.Interfaces.Ontology.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.OntologyData.Migration
{
    public class MigrationReference : IMigrationReference
    {
        public string EntityName { get; set; }

        public string SourceDotName { get; set; }

        public string TargetDotName { get; set; }
    }
}
