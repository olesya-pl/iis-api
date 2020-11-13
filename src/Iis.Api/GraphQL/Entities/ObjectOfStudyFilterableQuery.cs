using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using HotChocolate;
using Iis.Domain;
using Iis.Interfaces.Ontology.Data;
using IIS.Core.GraphQL.Common;
using IIS.Core.GraphQL.Entities.InputTypes;

namespace IIS.Core.GraphQL.Entities
{
    public class ObjectOfStudyFilterableQuery
    {
        public async Task<ObjectOfStudyFilterableQueryResponse> EntityObjectOfStudyFilterableList(
            [Service] IOntologyService ontologyService,            
            [Service] IOntologyNodesData nodesData,
            [Service] IMapper mapper,
            PaginationInput pagination,
            AllEntitiesFilterInput filter
            )
        {
            var types = filter.Types is null || !filter.Types.Any() ? new[] { "ObjectOfStudy" } : filter.Types;

            var response = await ontologyService.FilterNodeAsync(types, new ElasticFilter
            {
                Limit = pagination.PageSize,
                Offset = pagination.Offset(),
                Suggestion = filter?.Suggestion ?? filter?.SearchQuery
            });
            var mapped = mapper.Map<ObjectOfStudyFilterableQueryResponse>(response);
            mapped.Aggregations = EnrichWithNodeTypeNames(nodesData, mapped.Aggregations);
            return mapped;
        }

        [Pure]
        private static Dictionary<string, AggregationItem> EnrichWithNodeTypeNames(IOntologyNodesData nodesData, 
            Dictionary<string, AggregationItem> aggregations)
        {
            var res = new Dictionary<string, AggregationItem>(aggregations);
            if (aggregations.ContainsKey("NodeTypeTitle"))
            {
                var nodeTypes = nodesData.Schema.GetAllNodeTypes();
                var titleAggregation = res["NodeTypeTitle"];
                foreach (var bucket in titleAggregation.Buckets)
                {
                    var nodeTypeName = nodeTypes
                        .FirstOrDefault(p => string.Equals(p.Title, bucket.Key, System.StringComparison.Ordinal))?
                        .Name;
                    if (!string.IsNullOrEmpty(nodeTypeName))
                    {
                        bucket.TypeName = $"Entity{nodeTypeName}";
                    }

                }
            }
            return res;
        }
    }
}
