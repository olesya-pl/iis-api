using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.DataModel
{
    public class OntologyMigrationsEntity : BaseEntity
    {
        public int OrderNumber { get; set; }
        public string StructureBefore { get; set; }
        public string StructureAfter { get; set; }
        public string MigrationRules { get; set; }
        public string Log { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public bool IsSuccess { get; set; }
    }
}
