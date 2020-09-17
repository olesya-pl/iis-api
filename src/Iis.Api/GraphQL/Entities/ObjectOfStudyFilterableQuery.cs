using System.Linq;
using System.Threading.Tasks;
using HotChocolate;
using Iis.Domain;
using IIS.Core.GraphQL.Common;
using IIS.Core.GraphQL.Entities.InputTypes;

namespace IIS.Core.GraphQL.Entities
{
    public class ObjectOfStudyFilterableQuery
    {
        public async Task<ObjectOfStudyFilterableQueryResponse> EntityObjectOfStudyFilterableList(
            [Service] IOntologyService ontologyService,
            PaginationInput pagination,
            AllEntitiesFilterInput filter
            )
        {
            var types = filter.Types is null || !filter.Types.Any() ? new [] {"ObjectOfStudy"} : filter.Types;

            var response = await ontologyService.FilterNodeAsync(types ,new ElasticFilter
            {
                Limit = pagination.PageSize,
                Offset = pagination.Offset(),
                Suggestion = filter?.Suggestion ?? filter?.SearchQuery
            });
            return new ObjectOfStudyFilterableQueryResponse
            {
                Items = response.nodes,
                Count = response.count
            };
        }
    }
}
