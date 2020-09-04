using Iis.Interfaces.Ontology.Data;
using Iis.OntologyData.DataTypes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.OntologyData.Migration
{
    public class MigrationItem: IMigrationItem
    {
        public string SourceDotName { get; set; }
        public string TargetDotName { get; set; }

        public MigrationItemOptions _options = new MigrationItemOptions();
        public IMigrationItemOptions Options => _options;
        public List<string> OtherTargetDotNames { get; set; }

        public MigrationItem() { }
        public MigrationItem(string sourceDotName, string targetDotName, MigrationItemOptions options = null)
        {
            SourceDotName = sourceDotName;
            TargetDotName = targetDotName;
            if (options != null)
            {
                _options = options;
            }
        }
    }
}
