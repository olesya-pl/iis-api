using Iis.Interfaces.Ontology.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.OntologyManager.UiControls
{
    public class MigrationOptions : IMigrationOptions
    {
        public bool SaveNewObjects { get; set; }

        public bool DeleteOldObjects { get; set; }
    }
}
