using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HotChocolate;
using Iis.Domain;
using Newtonsoft.Json.Linq;

namespace Iis.Api.Ontology
{
    public class AssociatedEventsQuery
    {
        public async Task<IEnumerable<EventAssociatedWithEntity>> GetEventsAssociatedWithEntity(
            [Service] IOntologyService ontologyService,
            [Service] NodeToJObjectMapper nodeToJObjectMapper,
            [GraphQLNonNullType] Guid entityId
        )
        {
            var entities = await ontologyService.GetEventsAssociatedWithEntity(entityId);

            return await Task.WhenAll(entities.Select(p => nodeToJObjectMapper.EventToAssociatedWithEntity(p)));
        }
    }
}
