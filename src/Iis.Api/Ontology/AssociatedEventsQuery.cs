using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HotChocolate;
using Iis.Domain;

namespace Iis.Api.Ontology
{
    public class AssociatedEventsQuery
    {
        public async Task<IEnumerable<EventsAssociatedWithEntityResponse>> GetEventsAssociatedWithEntity(
            [Service] IOntologyService ontologyService,
            [Service] NodeToJObjectMapper nodeToJObjectMapper,
            [GraphQLNonNullType] Guid entityId
        )
        {
            var entities = await ontologyService.GetEventsAssociatedWithEntity(entityId);

            return entities.Select(p => new EventsAssociatedWithEntityResponse {
                Event = nodeToJObjectMapper.EventToJObject(p)
            });
        }
    }
}
