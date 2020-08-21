using Iis.Interfaces.Ontology.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.OntologyData.DataTypes
{
    public class MigrationItemOptions : IMigrationItemOptions
    {
        public string IgnoreIfFieldsAreNotEmpty { get; set; }
        public bool Ignore { get; set; }
    }
}
