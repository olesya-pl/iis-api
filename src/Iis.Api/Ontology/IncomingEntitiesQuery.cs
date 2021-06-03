using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using HotChocolate;
using Iis.Domain;

namespace Iis.Api.Ontology
{
    public class IncomingEntitiesQuery
    {
        public IEnumerable<IncomingRelation> GetIncomingEntities([Service] IOntologyService ontologyService,
            [Service] IMapper mapper,
            [Service] Iis.Services.NodeToJObjectMapper nodeToJObjectMapper,
            Guid entityId)
        {
            var items = ontologyService.GetIncomingEntities(entityId);

            var res = items.Select(p => mapper.Map<IncomingRelation>(p)).ToList();

            foreach (var item in items)
            {
                var resItem = res.FirstOrDefault(p => p.EntityId == item.EntityId);
                if (resItem is null)
                {
                    continue;
                }
                resItem.Entity = nodeToJObjectMapper.NodeToJObject(item.Entity);
            }
            return res;
        }
    }
}
