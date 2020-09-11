using Iis.Interfaces.Ontology.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Iis.OntologyData.Migration
{
    public class MigrationEntity : IMigrationEntity
    {
        public string SourceEntityName { get; set; }
        public string TargetEntityName { get; set; }
        public List<string> LinkedEntities { get; set; }
        public IReadOnlyList<MigrationItem> Items { get; set; }
        public IReadOnlyList<IMigrationItem> GetItems(string sourceDotName)
        {
            return Items.Where(i => i.SourceDotName == sourceDotName).ToList();
        }
    }
}
