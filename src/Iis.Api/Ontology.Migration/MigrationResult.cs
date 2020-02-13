using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Iis.Api.Ontology.Migration
{
    public class MigrationResult
    {
        public string StructureBefore { get; set; }
        public string StructureAfter { get; set; }
        public string MigrationRules { get; set; }
        public string Log { get; set; }
        public bool IsSuccess { get; set; }
    }
}
