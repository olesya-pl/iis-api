using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HotChocolate;
using Iis.Domain;
using IIS.Services.Contracts.Interfaces;

namespace Iis.Api.Ontology
{
    public class RelationsCountQuery
    {
        public async Task<List<NodeRelationCount>> GetRelationsCounts(
            [Service] IOntologyService ontologyService,
            [Service] IMaterialProvider materialProvider,
            Guid[] entityIds)
        {
            var uniqueEntityIds = entityIds.ToHashSet();
            var relationsCount = ontologyService.GetRelationsCount(uniqueEntityIds);
            var eventsCount = ontologyService.CountEventsAssociatedWithEntities(uniqueEntityIds);
            var materialsCount = await materialProvider.CountMaterialsByNodeIds(uniqueEntityIds);
            return MergeResults(uniqueEntityIds, relationsCount, eventsCount, materialsCount);
        }

        private static List<NodeRelationCount> MergeResults(HashSet<Guid> uniqueEntityIds, Dictionary<Guid, int> relationsCount, Dictionary<Guid, int> eventsCount, Dictionary<Guid, int> materialsCount)
        {
            var res = new List<NodeRelationCount>();
            return uniqueEntityIds.Select(entityId => {
                var item = new NodeRelationCount()
                {
                    NodeId = entityId
                };
                if (relationsCount.ContainsKey(entityId))
                {
                    item.RelationsCount = relationsCount[entityId];
                }

                if (eventsCount.ContainsKey(entityId))
                {
                    item.EventsCount = eventsCount[entityId];
                }

                if (materialsCount.ContainsKey(entityId))
                {
                    item.MaterialsCount = materialsCount[entityId];
                }
                return item;
            }).ToList();
        }
    }
}
