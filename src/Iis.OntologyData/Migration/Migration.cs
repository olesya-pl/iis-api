using Iis.Interfaces.Ontology.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.OntologyData.Migration
{
    public class Migration : IMigration
    {
        public string Title { get; set; }

        public IReadOnlyList<MigrationEntity> Items;
        public IReadOnlyList<IMigrationEntity> GetItems() => Items;
    }
}
