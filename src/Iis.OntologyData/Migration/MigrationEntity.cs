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
        public IReadOnlyList<MigrationItem> Items { get; set; }
        public string GetTargetDotName(string sourceDotName)
        {
            return GetItem(sourceDotName)?.TargetDotName;
        }
        public IMigrationItem GetItem(string sourceDotName)
        {
            return Items.SingleOrDefault(i => i.SourceDotName == sourceDotName);
        }
    }
}
