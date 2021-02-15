﻿using Iis.Interfaces.Ontology.Schema;
using Iis.Services.Contracts.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System;
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
                    .ToArray();
            } else OntologyIndexes = Array.Empty<string>();

            var wikiType = ontologySchema.GetEntityTypeByName(EntityTypeNames.Wiki.ToString());
            if (wikiType != null)
            {
                WikiIndexes = wikiType.GetAllDescendants()
                    .Where(nt => !nt.IsAbstract)
                    .Select(nt => nt.Name)
                    .ToArray();
            } else WikiIndexes = Array.Empty<string>();

            ObjectIndexes = OntologyIndexes.Concat(WikiIndexes).ToArray();

            EventIndexes = new [] { "Event" };
            MaterialIndexes = new [] { "Materials" };
            ReportIndex = "Reports";
            SignIndexes = new [] { "CellphoneSign", "SatellitePhoneSign" };
            FieldsToExcludeByIndex = new Dictionary<string, IEnumerable<string>>()
            {
                { "Event", new [] { "associatedWithEvent" } }
            };
        }

        public string ReportIndex { get; set; }
        public IReadOnlyCollection<string> MaterialIndexes { get; }
        public IReadOnlyCollection<string> OntologyIndexes { get; }
        public IReadOnlyCollection<string> WikiIndexes { get; }
        public IReadOnlyCollection<string> ObjectIndexes { get; }
        public IReadOnlyCollection<string> EventIndexes { get; }
        public IReadOnlyCollection<string> SignIndexes { get; }
        public Dictionary<string, IEnumerable<string>> FieldsToExcludeByIndex { get; }
    }
}
