using Iis.Interfaces.Materials;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IIS.Core.GraphQL.Materials
{
    public class MaterialUpdateInput : IMaterialUpdateInput
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public Guid? ImportanceId { get; set; }
        public Guid? ReliabilityId { get; set; }
        public Guid? RelevanceId { get; set; }
        public Guid? CompletenessId { get; set; }
        public Guid? SourceReliabilityId { get; set; }
        public IEnumerable<string> Objects { get; set; }
        public IEnumerable<string> Tags { get; set; }
        public IEnumerable<string> States { get; set; }
    }
}
