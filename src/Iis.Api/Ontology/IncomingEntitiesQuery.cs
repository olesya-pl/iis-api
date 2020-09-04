using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using HotChocolate;
using Iis.Domain;

namespace Iis.Api.Ontology
{
    public class IncomingEntitiesQuery
    {
        public async Task<IEnumerable<IncomingRelation>> GetIncomingEntities([Service] IOntologyService ontologyService,
            [Service] IMapper mapper,
            Guid entityId)
        {
            var items = await ontologyService.GetIncomingEntities(entityId);

            return items.Select(p => mapper.Map<IncomingRelation>(p));
        }
    }
}
