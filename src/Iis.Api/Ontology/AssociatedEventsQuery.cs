using System;
using System.Linq;
using System.Threading.Tasks;
using HotChocolate;
using Iis.Domain;
using IIS.Core.GraphQL.Common;

namespace Iis.Api.Ontology
{
    public class AssociatedEventsQuery
    {
        public async Task<GraphQLCollection<EventAssociatedWithEntity>> GetEventsAssociatedWithEntity(
            [Service] IOntologyService ontologyService,
            [Service] NodeToJObjectMapper nodeToJObjectMapper,
            [GraphQLNonNullType] Guid entityId
        )
        {
            var entities = await ontologyService.GetEventsAssociatedWithEntity(entityId);

            var res = await Task.WhenAll(entities.Select(p => nodeToJObjectMapper.EventToAssociatedWithEntity(p)));
            return new GraphQLCollection<EventAssociatedWithEntity>(res, res.Count());
        }
    }
}
