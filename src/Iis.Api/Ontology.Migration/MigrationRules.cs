using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Iis.Api.Ontology.Migration
{
    public class MigrationRules
    {
        public int OrderNumber { get; set; }
        public List<string> AllowedEntities { get; set; }
        public TypeMappings DirectMappings { get; set; }
        public List<MigrationCondition> Conditions { get; set; }
        public bool MigratePersonNames { get; set; }
    }
}
