using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using HotChocolate;
using Iis.Api.GraphQL.Common;
using Iis.Domain;
using Iis.Interfaces.Ontology.Data;
using Iis.Interfaces.Ontology.Schema;
using Iis.Interfaces.Elastic;
using IIS.Core.GraphQL.Common;
using IIS.Core.GraphQL.Entities.InputTypes;
using Iis.Services.Contracts.Params;
using Iis.Utility;
using Newtonsoft.Json.Linq;
using HotChocolate.Resolvers;

namespace IIS.Core.GraphQL.Entities
{
    public class OntologyFilterableQuery
    {
        private static readonly string[] ObjectOfStudyTypeList = new[] { EntityTypeNames.ObjectOfStudy.ToString()};
        private static readonly string[] EntityList = new[] { EntityTypeNames.ObjectOfStudy.ToString(), EntityTypeNames.Wiki.ToString() };
        public async Task<OntologyFilterableQueryResponse> EntityObjectOfStudyFilterableList(
            IResolverContext ctx,
            [Service] IOntologyService ontologyService,
            [Service] IOntologyNodesData nodesData,
            [Service] IMapper mapper,
            PaginationInput pagination,
            AllEntitiesFilterInput filter
            )
        {
            var tokenPayload = ctx.ContextData[TokenPayload.TokenPropertyName] as TokenPayload;
            var types = filter.Types is null || !filter.Types.Any() ? EntityList : filter.Types;

            var elasticFilter = new ElasticFilter
            {
                Limit = pagination.PageSize,
                Offset = pagination.Offset(),
                Suggestion = filter?.Suggestion ?? filter?.SearchQuery,
                CherryPickedItems = filter.CherryPickedItems.ToList(),
                FilteredItems = filter.FilteredItems
            };
            var response = await ontologyService.FilterNodeAsync(types, elasticFilter, tokenPayload.UserId);
            var mapped = mapper.Map<OntologyFilterableQueryResponse>(response);
            EnrichWithSelectedFilteredItems(mapped.Aggregations, elasticFilter);
            mapped.Aggregations = EnrichWithNodeTypeNames(nodesData, mapped.Aggregations);
            
            return mapped;
        }

        public async Task<OntologyFilterableQueryResponse> EntityObjectOfStudyCoordinatesList(
            [Service] IOntologyService ontologyService,
            [Service] IMapper mapper,
            PaginationInput pagination,
            AllEntitiesFilterInput filter
            )
        {
            var types = filter.Types is null || !filter.Types.Any() ? ObjectOfStudyTypeList : filter.Types;

            var response = await ontologyService.FilterNodeCoordinatesAsync(types, new ElasticFilter
            {
                Limit = pagination.PageSize,
                Offset = pagination.Offset(),
                Suggestion = filter?.Suggestion ?? filter?.SearchQuery
            });
            var mapped = mapper.Map<OntologyFilterableQueryResponse>(response);
            return mapped;
        }

        public async Task<OntologyFilterableQueryResponse> GetEventList(
            [Service] IOntologyService ontologyService,
            [Service] IMapper mapper,
            PaginationInput pagination,
            FilterInput filter,
            SortingInput sorting
        )
        {
            var sortingParam = mapper.Map<SortingParams>(sorting) ?? SortingParams.Default;

            var response = await ontologyService.SearchEventsAsync(new ElasticFilter
            {
                Limit = pagination.PageSize,
                Offset = pagination.Offset(),
                Suggestion = filter?.Suggestion ?? filter?.SearchQuery,
                SortColumn = sortingParam.ColumnName,
                SortOrder = sortingParam.Order
            });

            return new OntologyFilterableQueryResponse()
            {
                Count = response.Count,
                Items = response.Entities.Select(ToCamelCase)
            };
        }

        //TODO: temporary solution, should be removed when all fields in elastic becomes in camel case 
        private JObject ToCamelCase(JObject original)
        {
            var newObj = new JObject();
            foreach (var property in original.Properties())
            {
                var newPropertyName = ToCamelCaseString(property.Name);
                newObj[newPropertyName] = property.Value;
            }

            return newObj;
        }

        private string ToCamelCaseString(string str)
        {
            if (!string.IsNullOrEmpty(str))
            {
                return char.ToLowerInvariant(str[0]) + str.Substring(1);
            }

            return str;
        }

        [Pure]
        private static Dictionary<string, AggregationItem> EnrichWithNodeTypeNames(IOntologyNodesData nodesData, 
            Dictionary<string, AggregationItem> aggregations)
        {
            var res = new Dictionary<string, AggregationItem>(aggregations);
            if (aggregations.ContainsKey(ElasticConfigConstants.NodeTypeTitleAlias))
            {
                var nodeTypes = nodesData.Schema.GetAllNodeTypes();
                var titleAggregation = res[ElasticConfigConstants.NodeTypeTitleAlias];
                foreach (var bucket in titleAggregation.Buckets)
                {
                    var nodeTypeName = nodeTypes
                        .FirstOrDefault(p => string.Equals(p.Title, bucket.Key, System.StringComparison.Ordinal))?
                        .Name;
                    if (!string.IsNullOrEmpty(nodeTypeName))
                    {
                        bucket.TypeName = $"Entity{nodeTypeName.Capitalize()}";
                    }

                }
            }
            return res;
        }

        private static void EnrichWithSelectedFilteredItems(Dictionary<string, AggregationItem> aggregations, ElasticFilter filter)
        {
            var selectedItems = filter.FilteredItems.GroupBy(x => x.Name).ToDictionary(x => x.Key, x => x.Select(i => i.Value));
            foreach (var item in selectedItems)
            {
                if (!aggregations.ContainsKey(item.Key))
                    continue;
                
                foreach (var value in item.Value)
                {
                    if (aggregations[item.Key].Buckets.All(x => x.Key != value))
                    {
                        aggregations[item.Key].Buckets.Add(new AggregationBucket()
                        {
                            DocCount = 0,
                            Key = value
                        });
                    }
                }
            }
        }
    }
}
