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
            }

            var wikiType = ontologySchema.GetEntityTypeByName(EntityTypeNames.Wiki.ToString());
            if (wikiType != null)
            {
                WikiIndexes = wikiType.GetAllDescendants()
                    .Where(nt => !nt.IsAbstract)
                    .Select(nt => nt.Name)
                    .ToList();
            }

            EventIndexes = new List<string> { "Event" };
            MaterialIndexes = new List<string> { "Materials" };
            ReportIndex = "Reports";
            SignIndexes = new List<string> { "CellphoneSign", "SatellitePhoneSign" };
            FieldsToExcludeByIndex = new Dictionary<string, IEnumerable<string>>()
            {
                { "Event", new [] { "associatedWithEvent" } }
            };
        }

        public string ReportIndex { get; set; }
        public List<string> MaterialIndexes { get; }
        public List<string> OntologyIndexes { get; }
        public List<string> WikiIndexes { get; }
        public List<string> EventIndexes { get; }
        public List<string> SignIndexes { get; }
        public Dictionary<string, IEnumerable<string>> FieldsToExcludeByIndex { get; }
    }
}
