using Iis.Interfaces.Ontology.Schema;
using Iis.Services.Contracts.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace Iis.Services
{
    public class ElasticState : IElasticState
    {
        public ElasticState(IOntologySchema ontologySchema)
        {
            var objectOfStudyType = ontologySchema.GetEntityTypeByName(EntityTypeNames.ObjectOfStudy.ToString());
            if (objectOfStudyType != null)
            {
                OntologyIndexes = objectOfStudyType.GetAllDescendants()
                    .Where(nt => !nt.IsAbstract)
                    .Select(nt => nt.Name)
                    .ToList();

                HistoricalOntologyIndexes = OntologyIndexes.ToDictionary(k => k, GetHistoricalIndex);
            }

            EventIndexes = new List<string> { "Event" };
            MaterialIndexes = new List<string> { "Materials" };
            ReportIndex = "Reports";
            SignIndexes = new List<string>{ "CellphoneSign", "SatellitePhoneSign"};
        }

        public string ReportIndex { get; set; }
        public List<string> MaterialIndexes { get; }
        public List<string> OntologyIndexes { get; }
        public Dictionary<string, string> HistoricalOntologyIndexes { get; }
        public List<string> EventIndexes { get; }
        public List<string> SignIndexes { get; }

        private string GetHistoricalIndex(string typeName) => $"historical_{typeName}";
    }
}
