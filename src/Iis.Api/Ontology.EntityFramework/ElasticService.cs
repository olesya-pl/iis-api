using Iis.DbLayer.Repositories;
using Iis.Domain.Elastic;
using Iis.Elastic.SearchQueryExtensions;
using Iis.Interfaces.Elastic;
using Iis.Interfaces.Ontology.Data;
using Iis.Services.Contracts.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace IIS.Core.Ontology.EntityFramework
{
    public class ElasticService : IElasticService
    {
        private readonly IElasticManager _elasticManager;
        private readonly IElasticConfiguration _elasticConfiguration;
        private readonly INodeRepository _nodeRepository;
        private readonly IElasticState _elasticState;
        private const decimal HistoricalSearchBoost = 0.05m;

        public ElasticService(
            IElasticManager elasticManager,
            IElasticConfiguration elasticConfiguration,
            INodeRepository nodeRepository,
            IElasticState elasticState)
        {
            _elasticManager = elasticManager;
            _elasticConfiguration = elasticConfiguration;
            _nodeRepository = nodeRepository;
            _elasticState = elasticState;
        }

        public Task<int> CountByAllFieldsAsync(IEnumerable<string> typeNames, ElasticFilter filter, CancellationToken ct = default)
        {
            var searchParams = new IisElasticSearchParams
            {
                BaseIndexNames = typeNames.ToList(),
                Query = $"*{filter.Suggestion}*"
            };

            return _elasticManager.CountAsync(searchParams, ct);
        }

        public async Task<SearchResult> SearchByConfiguredFieldsAsync(IEnumerable<string> typeNames, ElasticFilter filter, CancellationToken ct = default)
        {
            var ontologyFields
                = _elasticConfiguration.GetOntologyIncludedFields(typeNames.Where(p => _elasticState.ObjectIndexes.Contains(p)));

            var searchParams = new IisElasticSearchParams
            {
                BaseIndexNames = typeNames.ToList(),
                Query = string.IsNullOrEmpty(filter.Suggestion) ? "*" : $"{filter.Suggestion}",
                From = filter.Offset,
                Size = filter.Limit,
                SearchFields = ontologyFields,
                SortColumn = filter.SortColumn,
                SortOrder = filter.SortOrder
            };

            var searchResult = await _elasticManager.SearchAsync(searchParams, ct);

            return new SearchResult
            {
                Count = searchResult.Count,
                Items = searchResult.Items
                    .ToDictionary(k => new Guid(k.Identifier),
                    v => new SearchResultItem { Highlight = v.Higlight, SearchResult = v.SearchResult })
            };
        }

        public Task<int> CountByConfiguredFieldsAsync(IEnumerable<string> typeNames, ElasticFilter filter, CancellationToken ct = default)
        {
            var ontologyFields
                = _elasticConfiguration.GetOntologyIncludedFields(typeNames.Where(p => _elasticState.OntologyIndexes.Contains(p)));

            var searchParams = new IisElasticSearchParams
            {
                BaseIndexNames = typeNames.ToList(),
                Query = string.IsNullOrEmpty(filter.Suggestion) ? "*" : $"{filter.Suggestion}",
                From = filter.Offset,
                Size = filter.Limit,
                SearchFields = ontologyFields
            };
            return _elasticManager.CountAsync(searchParams, ct);
        }

        private bool ShouldReturnAllEntities(ElasticFilter filter)
        {
            return SearchQueryExtension.IsMatchAll(filter.Suggestion) 
                   && !filter.FilteredItems.Any() 
                   && !filter.CherryPickedItems.Any();
        }

        public async Task<SearchEntitiesByConfiguredFieldsResult> SearchEntitiesByConfiguredFieldsAsync(
            IEnumerable<string> typeNames, 
            ElasticFilter filter,
            Guid userId,
            CancellationToken ct = default)
        {
            if(ShouldReturnAllEntities(filter))
            {
                var defaultAggregations = new List<AggregationField>
                {
                    new AggregationField(
                        ElasticConfigConstants.NodeTypeTitleAggregateField,
                        ElasticConfigConstants.NodeTypeTitleAlias,
                        ElasticConfigConstants.NodeTypeTitleAggregateField)
                };
                
                var query = new MatchAllQueryBuilder()
                    .WithPagination(filter.Offset, filter.Limit)
                    .BuildSearchQuery()
                    .WithAggregation(defaultAggregations)
                    .WithHighlights()
                    .ToString();
                
                var results = await _elasticManager.WithUserId(userId).SearchAsync(query, typeNames, ct);
                return results.ToOutputSearchResult();
            }

            var (multiSearchParams, historicalResult) = await PrepareMultiElasticSearchParamsAsync(typeNames, filter, ct);

            var aggregationFieldList = multiSearchParams.SearchParams.SelectMany(p => p.Fields)
                                    .Where(p => p.IsAggregated)
                                    .Select(e => new AggregationField($"{e.Name}{SearchQueryExtension.AggregateSuffix}", e.Alias, $"{e.Name}{SearchQueryExtension.AggregateSuffix}", e.Name))
                                    .ToArray();

            var multiSearchQuery = new MultiSearchParamsQueryBuilder(multiSearchParams.SearchParams)
                .WithLeniency(multiSearchParams.IsLenient)
                .WithPagination(multiSearchParams.From, multiSearchParams.Size)
                .WithResultFields(multiSearchParams.ResultFields)
                .BuildSearchQuery()
                .WithHighlights()
                .ToString();
            
            var aggregationQuery = new MatchAllQueryBuilder()
                .WithPagination(0, 0)
                .BuildSearchQuery()
                .WithAggregation(aggregationFieldList, filter)
                .ToString();

            var searchResult = await _elasticManager.SearchAsync(multiSearchQuery, typeNames, ct);
            var aggregationResult = await _elasticManager.SearchAsync(aggregationQuery, typeNames, ct);

            if (historicalResult != null && historicalResult.Count > 0)
            {
                var highlightsById = historicalResult.Items
                    .GroupBy(x => x.SearchResult["Id"].Value<string>())
                    .ToDictionary(k => k.Key, v => v.First().Higlight);

                foreach (var item in searchResult.Items)
                {
                    item.SearchResult["highlight"] = CombineHighlights(
                        highlightsById.GetValueOrDefault(item.Identifier),
                        item.Higlight,
                        item.Identifier);
                }
            }

            return searchResult.ToOutputSearchResult(ExtractSubAggregations(aggregationResult.Aggregations));
        }

        private Dictionary<string, AggregationItem> ExtractSubAggregations(Dictionary<string, AggregationItem> aggregations)
        {
            return aggregations.ToDictionary(x => x.Key, pair => pair.Value.SubAggs);
        }

        public async Task<SearchEntitiesByConfiguredFieldsResult> FilterNodeCoordinatesAsync(
            IEnumerable<string> typeNames,
            ElasticFilter filter,
            CancellationToken ct = default)
        {
            var (multiSearchParams, _) = await PrepareMultiElasticSearchParamsAsync(typeNames, filter, ct);

            var query = new MultiSearchParamsQueryBuilder(multiSearchParams.SearchParams)
                .WithPagination(multiSearchParams.From, multiSearchParams.Size)
                .WithLeniency(multiSearchParams.IsLenient)
                .WithResultFields(multiSearchParams.ResultFields)
                .BuildSearchQuery()
                .ToString();

            var searchResult = await _elasticManager.SearchAsync(query, typeNames, ct);

            return searchResult.ToOutputSearchResult();
        }

        public async Task<int> CountEntitiesByConfiguredFieldsAsync(
            IEnumerable<string> typeNames,
            ElasticFilter filter,
            CancellationToken ct = default)
        {
            var (multiSearchParams, _)= await PrepareMultiElasticSearchParamsAsync(typeNames, filter, ct);
            var query = new MultiSearchParamsQueryBuilder(multiSearchParams.SearchParams)
                .WithLeniency(multiSearchParams.IsLenient)
                .BuildCountQuery()                
                .ToString();
            return await _elasticManager.CountAsync(query, typeNames, ct);
        }
        
        private async Task<(MultiElasticSearchParams MultiSearchParams, IElasticSearchResult HistoricalResult)> PrepareMultiElasticSearchParamsAsync(IEnumerable<string> typeNames, ElasticFilter filter, CancellationToken ct = default) 
        {
            var useHistoricalSearch = !string.IsNullOrEmpty(filter.Suggestion);

            var searchFields = _elasticConfiguration
                        .GetOntologyIncludedFields(typeNames.Where(p => _elasticState.ObjectIndexes.Contains(p)))
                        .ToList();

            var multiSearchParams = new MultiElasticSearchParams
            {
                BaseIndexNames = typeNames.ToList(),
                From = filter.Offset,
                Size = filter.Limit,
                SearchParams = new List<(string Query, List<IIisElasticField> Fields)>
                {
                    (ShouldReturnAllEntities(filter) ? "*" : $"{filter.ToQueryString()}", searchFields)
                }
            };
            
            if(!useHistoricalSearch)
                return (multiSearchParams, null);
            
            IElasticSearchResult historySearchResult;
            
            var historicalIndexes = typeNames.Select(GetHistoricalIndex).ToList();
            if (SearchQueryExtension.IsExactQuery(filter.Suggestion))
            {
                var exactQuery = new ExactQueryBuilder()
                    .WithResultFields(new List<string> { "Id" })
                    .WithPagination(0, filter.Limit)
                    .WithQueryString(filter.Suggestion)
                    .WithLeniency(true)
                    .BuildSearchQuery();

                historySearchResult = await _elasticManager.SearchAsync(exactQuery.ToString(), historicalIndexes, ct);
            }
            else
            {
                var historySearchParams = new IisElasticSearchParams
                {
                    BaseIndexNames = historicalIndexes,
                    Query = filter.ToQueryString(),
                    From = 0,
                    Size = filter.Limit,
                    SearchFields = searchFields,
                    ResultFields = new List<string> { "Id" }
                };

                historySearchResult = await _elasticManager.SearchAsync(historySearchParams, ct);
            }

            if (historySearchResult.Count > 0)
            {
                var entityIds = historySearchResult.Items
                    .Select(x => x.SearchResult["Id"].Value<string>())
                    .Distinct();

                multiSearchParams.SearchParams.Add((string.Join(" ", entityIds),
                    new List<IIisElasticField>
                    {
                        new IisElasticField
                        {
                            Name = "Id",
                            Boost = HistoricalSearchBoost
                        }
                    }));
            }

            return (multiSearchParams, historySearchResult);
        }

        private JToken CombineHighlights(JToken historicalHighlights, JToken actualHighlights, string entityId)
        {
            if (historicalHighlights == null)
                return actualHighlights;

            var result = DeepCloneWithNewPrefix(historicalHighlights, "historical");
            if (actualHighlights == null)
                return result;

            foreach (var item in ((JObject)actualHighlights).Children<JProperty>())
            {
                if (item.Name == "Id" && item.Value[0].Value<string>().Contains(entityId))
                    continue;

                result.TryAdd(item.Name, item.Value);
            }

            return result;
        }

        private JObject DeepCloneWithNewPrefix(JToken token, string prefix)
        {
            var result = new JObject();
            foreach (var jProp in token.Children<JProperty>())
            {
                result.Add($"{prefix}.{jProp.Name}", jProp.Value);
            }

            return result;
        }

        private string GetHistoricalIndex(string typeName)
        {
            return $"historical_{typeName}";
        }

        public Task<bool> PutNodeAsync(Guid id, CancellationToken ct = default)
        {
            return _nodeRepository.PutNodeAsync(id, ct);
        }

        public Task<bool> PutNodeAsync(Guid id, IEnumerable<string> fieldsToExtract, CancellationToken ct = default)
        {
            return _nodeRepository.PutNodeAsync(id, fieldsToExtract, ct);
        }

        public Task<List<ElasticBulkResponse>> PutHistoricalNodesAsync(Guid id, Guid? requestId = null, CancellationToken ct = default)
        {
            return _nodeRepository.PutHistoricalNodesAsync(id, requestId, ct);
        }

        public bool TypesAreSupported(IEnumerable<string> typeNames)
        {
            if(typeNames is null || !typeNames.Any()) return false;

            return OntologyIndexesAreSupported(typeNames);
        }

        private bool OntologyIndexIsSupported(string indexName)
        {
            return _elasticState.ObjectIndexes.Any(index => index.Equals(indexName))
                || _elasticState.EventIndexes.Any(index => index.Equals(indexName));
        }

        private bool OntologyIndexesAreSupported(IEnumerable<string> indexNames)
        {
            return indexNames.All(indexName => OntologyIndexIsSupported(indexName));
        }

        public async Task<bool> PutNodesAsync(IReadOnlyCollection<INode> itemsToUpdate, CancellationToken ct)
        {
            var response = await _nodeRepository.PutNodesAsync(itemsToUpdate, ct);

            return response.All(x => x.IsSuccess);
        }

        public async Task<IEnumerable<IElasticSearchResultItem>> SearchByFieldsAsync(string query, IReadOnlyCollection<string> fieldNames, IReadOnlyCollection<string> typeNames, int size, CancellationToken ct = default)
        {
            var searchParams = new IisElasticSearchParams
            {
                BaseIndexNames = typeNames,
                Query = query,
                Size = size,
                SearchFields = fieldNames.Select(x => new IisElasticField { Name = x}).ToArray()
            };
            var searchResult = await _elasticManager.SearchAsync(searchParams, ct);
            return searchResult.Items;
        }

        public async Task<SearchResult> SearchSignsAsync(IEnumerable<string> typeNames, ElasticFilter filter, CancellationToken ct = default)
        {
            var queryData = new ExactQueryBuilder()
                .WithPagination(filter.Offset, filter.Limit)
                .WithQueryString(filter.Suggestion)
                .BuildSearchQuery()
                .SetupSorting("CreatedAt", "asc")
                .ToString(Formatting.None);

            var searchResult = await _elasticManager.SearchAsync(queryData, typeNames, ct);

            return new SearchResult
            {
                Count = searchResult.Count,
                Items = searchResult.Items
                    .ToDictionary(k => new Guid(k.Identifier),
                    v => new SearchResultItem { Highlight = v.Higlight, SearchResult = v.SearchResult })
            };
        }
        public Task<bool> DeleteNodeAsync(Guid id, string typeName, CancellationToken ct = default)
        {
            var indexName = _elasticState.OntologyIndexes.Union(_elasticState.EventIndexes).FirstOrDefault(e => e.Equals(typeName, StringComparison.OrdinalIgnoreCase));

            return _elasticManager.DeleteDocumentAsync(indexName, id.ToString("N"), ct);
        }
    }
}
