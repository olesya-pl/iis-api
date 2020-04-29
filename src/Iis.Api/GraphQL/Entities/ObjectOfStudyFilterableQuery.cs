using System.Collections.Generic;
using System.Threading.Tasks;
using HotChocolate;
using HotChocolate.Types;
using Iis.Domain;
using IIS.Core.GraphQL.Common;
using IIS.Core.GraphQL.Entities.InputTypes;
using IIS.Core.GraphQL.Scalars;
using Newtonsoft.Json.Linq;

namespace IIS.Core.GraphQL.Entities
{
    public class ObjectOfStudyFilterableQueryReponse
    {
        [GraphQLType(typeof(ListType<JsonScalarType>))]
        public IEnumerable<JObject> Items { get; set; }
        public int Count { get; set; }
    }

    public class ObjectOfStudyFilterableQuery
    {
        public async Task<ObjectOfStudyFilterableQueryReponse> EntityObjectOfStudyFilterableList(
            [Service] IOntologyService ontologyService,
            PaginationInput pagination,
            FilterInput filter
            )
        {
            var response = await ontologyService.FilterObjectsOfStudyAsync(new ElasticFilter
            {
                Limit = pagination.PageSize,
                Offset = pagination.Offset(),
                Suggestion = filter.Suggestion ?? filter.SearchQuery
            });
            return new ObjectOfStudyFilterableQueryReponse
            {
                Items = response.nodes,
                Count = response.count
            };
        }
    }
}
