using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
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
            [Service] IMapper mapper,
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
            return mapper.Map<ObjectOfStudyFilterableQueryResponse>(response);            
        }
    }
}
