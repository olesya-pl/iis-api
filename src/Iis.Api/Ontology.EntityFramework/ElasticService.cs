using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using Iis.Domain.Elastic;
using Iis.Elastic.SearchQueryExtensions;
using Iis.Services.Contracts.Interfaces;
using Iis.Interfaces.Elastic;
using Iis.Interfaces.Ontology.Data;
using Iis.Interfaces.Ontology.Schema;
using Iis.Utility;

namespace IIS.Core.Ontology.EntityFramework
{
    public class ElasticService : IElasticService
    {
        private const decimal HistoricalSearchBoost = 0.05m;
        private const string ExclamationMark = "!";
        private const string IdField = "Id";
        private const string IdFieldSeparator = " ";
        private const string HistoricalIndexName = "historical";
        private const string Highlight = "highlight";
        private const int MaxAggregationsCount = 20;

        private readonly IElasticManager _elasticManager;
        private readonly IElasticConfiguration _elasticConfiguration;
        private readonly INodeSaveService _nodeRepository;
        private readonly IElasticState _elasticState;
        private readonly IGroupedAggregationNameGenerator _groupedAggregationNameGenerator;

        public ElasticService(
            IElasticManager elasticManager,
            IElasticConfiguration elasticConfiguration,
            INodeSaveService nodeRepository,
            IElasticState elasticState,
            IGroupedAggregationNameGenerator groupedAggregationNameGenerator)
        {
            _elasticManager = elasticManager;
            _elasticConfiguration = elasticConfiguration;
            _nodeRepository = nodeRepository;
            _elasticState = elasticState;
            _groupedAggregationNameGenerator = groupedAggregationNameGenerator;
        }

        public Task<int> CountByAllFieldsAsync(IEnumerable<string> typeNames, ElasticFilter filter, CancellationToken ct = default)
        {
            var queryExpression = string.IsNullOrEmpty(filter.Suggestion) ? SearchQueryExtension.Wildcard : $"{filter.Suggestion}";

            var searchParams = new IisElasticSearchParams
            {
                BaseIndexNames = typeNames.ToList(),
                Query = SearchQueryExtension.IsExactQuery(queryExpression) ? queryExpression : $"*{queryExpression}*",
                IsExact = filter.IsExact
            };

            return _elasticManager.CountAsync(searchParams, ct);
        }

        public async Task<SearchResult> SearchByConfiguredFieldsAsync(IEnumerable<string> typeNames, ElasticFilter filter, Guid userId, CancellationToken ct = default)
        {
            var ontologyFields
                = _elasticConfiguration.GetOntologyIncludedFields(typeNames.Where(p => _elasticState.ObjectIndexes.Contains(p)));

            var searchParams = new IisElasticSearchParams
            {
                BaseIndexNames = typeNames.ToList(),
                Query = string.IsNullOrEmpty(filter.Suggestion) ? SearchQueryExtension.Wildcard : $"{filter.Suggestion}",
                From = filter.Offset,
                Size = filter.Limit,
                SearchFields = ontologyFields,
                SortColumn = filter.SortColumn,
                SortOrder = filter.SortOrder,
                IsExact = filter.IsExact
            };

            var searchResult = await _elasticManager.WithUserId(userId).SearchAsync(searchParams, ct);

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
                Query = string.IsNullOrEmpty(filter.Suggestion) ? SearchQueryExtension.Wildcard : $"{filter.Suggestion}",
                From = filter.Offset,
                Size = filter.Limit,
                SearchFields = ontologyFields,
                IsExact = filter.IsExact
            };
            return _elasticManager.CountAsync(searchParams, ct);
        }

        public bool ShouldReturnAllEntities(ElasticFilter filter)
        {
            return SearchQueryExtension.IsMatchAll(filter.Suggestion)
                   && !filter.FilteredItems.Any()
                   && !filter.CherryPickedItems.Any();
        }
        public bool ShouldReturnNoEntities(ElasticFilter filter)
        {
            return filter.Suggestion?.Trim() == ExclamationMark;
        }

        public async Task<SearchEntitiesByConfiguredFieldsResult> SearchEntitiesByConfiguredFieldsAsync(
            IEnumerable<string> typeNames,
            ElasticFilter filter,
            Guid userId,
            CancellationToken ct = default)
        {
            if (ShouldReturnAllEntities(filter))
            {
                var result = await GetAllSearchResultAsync(typeNames, filter, userId, ct);

                return result.ToOutputSearchResult();
            }

            var (multiSearchQuery, context) = await PrepareMultiSearchQuery(typeNames, filter, ct);
            var aggregations = await GetAggregationsQueryResultAsync(typeNames, filter, userId, context, ct);
            var multiSearchQueryString = multiSearchQuery
                .WithHighlights()
                .ToString();
            var searchResult = await _elasticManager.WithUserId(userId).SearchAsync(multiSearchQueryString, typeNames, ct);

            PopulateHighlights(searchResult, context);

            return searchResult.ToOutputSearchResult(aggregations);
        }

        public async Task<SearchEntitiesByConfiguredFieldsResult> FilterNodeCoordinatesAsync(
            IEnumerable<string> typeNames,
            ElasticFilter filter,
            CancellationToken ct = default)
        {
            var context = await PrepareMultiElasticSearchContextAsync(typeNames, filter, ct);

            var query = new MultiSearchParamsQueryBuilder(context.MultiSearchParams.SearchParams)
                .WithPagination(context.MultiSearchParams.From, context.MultiSearchParams.Size)
                .WithLeniency(context.MultiSearchParams.IsLenient)
                .WithResultFields(context.MultiSearchParams.ResultFields)
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
            var context = await PrepareMultiElasticSearchContextAsync(typeNames, filter, ct);
            var query = new MultiSearchParamsQueryBuilder(context.MultiSearchParams.SearchParams)
                .WithLeniency(context.MultiSearchParams.IsLenient)
                .BuildCountQuery()
                .ToString();
            return await _elasticManager.CountAsync(query, typeNames, ct);
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
            if (typeNames is null || !typeNames.Any()) return false;

            return OntologyIndexesAreSupported(typeNames);
        }

        public async Task<bool> PutNodesAsync(IReadOnlyCollection<INode> itemsToUpdate, CancellationToken ct)
        {
            var response = await _nodeRepository.PutNodesAsync(itemsToUpdate, ct);

            return response.All(x => x.IsSuccess);
        }

        public async Task<IEnumerable<IElasticSearchResultItem>> SearchByFieldsAsync(string query, IReadOnlyCollection<string> fieldNames, IReadOnlyCollection<string> typeNames, int size, Guid userId, CancellationToken ct = default)
        {
            var searchParams = new IisElasticSearchParams
            {
                BaseIndexNames = typeNames,
                Query = query,
                Size = size,
                SearchFields = fieldNames.Select(x => new IisElasticField { Name = x }).ToArray(),
                IsExact = SearchQueryExtension.IsExactQuery(query)
            };
            var searchResult = await _elasticManager
                .WithUserId(userId)
                .SearchAsync(searchParams, ct);
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

        public bool TypeIsAvalilable(INodeTypeLinked type, bool entitySearchGranted, bool wikiSearchGranted)
        {
            return (entitySearchGranted && _elasticState.OntologyIndexes.Contains(type.Name))
                || (wikiSearchGranted && _elasticState.WikiIndexes.Contains(type.Name))
                || (!_elasticState.WikiIndexes.Contains(type.Name) && !_elasticState.OntologyIndexes.Contains(type.Name));
        }

        private async Task<(JObject Query, MultiSearchQueryContext Context)> PrepareMultiSearchQuery(
            IEnumerable<string> typeNames,
            ElasticFilter filter,
            CancellationToken ct)
        {
            var context = await PrepareMultiElasticSearchContextAsync(typeNames, filter, ct);
            var query = PrepareMultiSearchQuery(context.MultiSearchParams);

            return (query, context);
        }

        private JObject PrepareMultiSearchQuery(IElasticMultiSearchParams multiSearchParams)
        {
            return new MultiSearchParamsQueryBuilder(multiSearchParams.SearchParams)
                .WithLeniency(multiSearchParams.IsLenient)
                .WithPagination(multiSearchParams.From, multiSearchParams.Size)
                .WithResultFields(multiSearchParams.ResultFields)
                .BuildSearchQuery();
        }

        private Dictionary<string, AggregationItem> ExtractSubAggregations(Dictionary<string, AggregationItem> aggregations)
        {
            if (aggregations == null)
            {
                return SearchResultsExtension.EmptyAggregation;
            }

            return aggregations.ToDictionary(x => x.Key, pair => pair.Value.SubAggs ?? pair.Value);
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

        private async Task<MultiSearchQueryContext> PrepareMultiElasticSearchContextAsync(IEnumerable<string> typeNames, ElasticFilter filter, CancellationToken ct = default)
        {
            var searchFields = _elasticConfiguration
                .GetOntologyIncludedFields(typeNames.Where(p => _elasticState.ObjectIndexes.Contains(p)))
                .ToList();
            var baseParameterQuery = filter.ToQueryString();
            var multiSearchParams = new ElasticMultiSearchParams
            {
                BaseIndexNames = typeNames.ToList(),
                From = filter.Offset,
                Size = filter.Limit,
                SearchParams = new List<SearchParameter>
                {
                    new SearchParameter(baseParameterQuery, searchFields, filter.IsExact)
                }
            };
            var useHistoricalSearch = !string.IsNullOrEmpty(filter.Suggestion);

            if (!useHistoricalSearch)
            {
                return MultiSearchQueryContext.CreateFrom(multiSearchParams, filter);
            }

            var historySearchResult = await GetHistorySearchResultAsync(typeNames, filter, searchFields, ct);

            PopulateHistorySearchParams(multiSearchParams, historySearchResult);

            return MultiSearchQueryContext.CreateFrom(multiSearchParams, filter, historySearchResult);
        }

        private void PopulateHistorySearchParams(IElasticMultiSearchParams multiSearchParams, IElasticSearchResult historySearchResult)
        {
            if (historySearchResult.Count == 0) return;

            var entityIds = historySearchResult.Items
                .Select(x => x.SearchResult[IdField].Value<string>())
                .Distinct();
            var historySearchQueryFields = new List<IIisElasticField>
            {
                new IisElasticField { Name = IdField, Boost = HistoricalSearchBoost }
            };
            var query = string.Join(IdFieldSeparator, entityIds);
            var historySearchParameter = new SearchParameter(query, historySearchQueryFields);

            multiSearchParams.SearchParams.Add(historySearchParameter);
        }

        private async Task<IElasticSearchResult> GetHistorySearchResultAsync(
            IEnumerable<string> typeNames,
            ElasticFilter filter,
            IReadOnlyList<IIisElasticField> searchFields,
            CancellationToken ct = default)
        {
            var historicalIndexes = typeNames.Select(GetHistoricalIndex);
            var resultFields = new List<string> { IdField };

            if (filter.IsExact)
            {
                var exactQuery = new ExactQueryBuilder()
                    .WithResultFields(resultFields)
                    .WithPagination(0, filter.Limit)
                    .WithQueryString(filter.Suggestion)
                    .WithLeniency(true)
                    .BuildSearchQuery();

                return await _elasticManager.SearchAsync(exactQuery.ToString(), historicalIndexes, ct);
            }

            var historySearchParams = new IisElasticSearchParams
            {
                BaseIndexNames = historicalIndexes,
                Query = filter.ToQueryString(),
                Size = filter.Limit,
                SearchFields = searchFields,
                ResultFields = resultFields,
                IsExact = filter.IsExact
            };

            return await _elasticManager.SearchAsync(historySearchParams, ct);
        }

        private JToken CombineHighlights(JToken historicalHighlights, JToken actualHighlights, string entityId)
        {
            if (historicalHighlights == null)
                return actualHighlights;

            var result = DeepCloneWithNewPrefix(historicalHighlights, HistoricalIndexName);
            if (actualHighlights == null)
                return result;

            foreach (var item in ((JObject)actualHighlights).Children<JProperty>())
            {
                if (item.Name == IdField && item.Value[0].Value<string>().Contains(entityId))
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
            return $"{HistoricalIndexName}_{typeName}";
        }

        private Task<IElasticSearchResult> GetAllSearchResultAsync(
            IEnumerable<string> typeNames,
            ElasticFilter filter,
            Guid userId,
            CancellationToken ct)
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

            return _elasticManager.WithUserId(userId).SearchAsync(query, typeNames, ct);
        }

        private async Task<Dictionary<string, AggregationItem>> GetAggregationsQueryResultAsync(
            IEnumerable<string> typeNames,
            ElasticFilter filter,
            Guid userId,
            MultiSearchQueryContext context,
            CancellationToken ct)
        {
            var aggregationFields = context.MultiSearchParams.SearchParams.SelectMany(_ => _.Fields)
                .Where(_ => _.IsAggregated)
                .Select(_ => new AggregationField($"{_.Name}{SearchQueryExtension.AggregateSuffix}", _.Alias, $"{_.Name}{SearchQueryExtension.AggregateSuffix}", _.Name))
                .ToArray();
            var aggregatesContext = SearchParamsContext.CreateAggregatesContextFrom(context.SearchContext, filter);
            var multiSearchAggregationQuery = aggregatesContext.IsBaseQueryMatchAll
                ? new MatchAllQueryBuilder()
                    .WithPagination(filter.Offset, filter.Limit)
                    .WithResultFields(aggregatesContext.ElasticMultiSearchParams.ResultFields)
                    .BuildSearchQuery()
                : PrepareMultiSearchQuery(aggregatesContext.ElasticMultiSearchParams);
            var batchQueries = GetBatchAggregateQueries(aggregationFields, multiSearchAggregationQuery, filter, context).ToArray();

            _elasticManager.WithUserId(userId);

            var aggregationQueryResults = await batchQueries.ForEachAsync(_ => _elasticManager.SearchAsync(_, typeNames, ct));
            var aggregations = aggregationQueryResults
                .SelectMany(_ => _.Aggregations)
                .ToDictionary(_ => _.Key, _ => _.Value);

            return ExtractSubAggregations(aggregations);
        }

        private IEnumerable<string> GetBatchAggregateQueries(
            IReadOnlyCollection<AggregationField> aggregationFields,
            JObject multiSearchAggregationQuery,
            ElasticFilter filter,
            MultiSearchQueryContext context)
        {
            var batchCount = aggregationFields.Count() / MaxAggregationsCount + 1;

            for (var batchIndex = 0; batchIndex < batchCount; batchIndex++)
            {
                var fieldsToAggregate = aggregationFields
                       .Skip(batchIndex * MaxAggregationsCount)
                       .Take(MaxAggregationsCount);

                yield return multiSearchAggregationQuery
                    .WithAggregation(fieldsToAggregate, filter, _groupedAggregationNameGenerator, context.SearchContext)
                    .ToString();
            }
        }

        private void PopulateHighlights(IElasticSearchResult searchResult, MultiSearchQueryContext context)
        {
            if (!context.HasHistoricalResult) return;

            foreach (var item in searchResult.Items)
            {
                item.SearchResult[Highlight] = CombineHighlights(
                    context.HighlightsById.GetValueOrDefault(item.Identifier),
                    item.Higlight,
                    item.Identifier);
            }
        }

        private class MultiSearchQueryContext
        {
            public IElasticSearchResult HistoricalResult { get; private set; }
            public Dictionary<string, JToken> HighlightsById { get; private set; }
            public ISearchParamsContext SearchContext { get; private set; }
            public IElasticMultiSearchParams MultiSearchParams => SearchContext.ElasticMultiSearchParams;
            public bool HasHistoricalResult => HistoricalResult != null && HistoricalResult.Count > 0;

            public static MultiSearchQueryContext CreateFrom(
                IElasticMultiSearchParams elasticMultiSearchParams,
                ElasticFilter filter,
                IElasticSearchResult historicalResult = default)
            {
                var highlightsById = historicalResult?.Items
                        .GroupBy(_ => _.SearchResult[IdField].Value<string>())
                        .ToDictionary(_ => _.Key, _ => _.First().Higlight)
                        ?? new Dictionary<string, JToken>();
                var aggregateHistoryResultIds = GetAggregateHistoryResultQueries(highlightsById, filter);

                return new MultiSearchQueryContext
                {
                    HistoricalResult = historicalResult,
                    HighlightsById = highlightsById,
                    SearchContext = SearchParamsContext.CreateFrom(elasticMultiSearchParams, aggregateHistoryResultIds)
                };
            }

            private static IReadOnlyDictionary<string, string> GetAggregateHistoryResultQueries(
                IReadOnlyDictionary<string, JToken> highlightsById,
                ElasticFilter filter)
            {
                if (highlightsById.Count == 0
                    || filter.FilteredItems.Count == 0) return new Dictionary<string, string>();

                var results = new Dictionary<string, string>(filter.FilteredItems.Count);
                var groupedByAggregation = filter.FilteredItems
                    .GroupBy(_ => _.Name)
                    .ToDictionary(_ => _.Key, _ => _.ToArray());
                var highlights = highlightsById
                    .Where(_ => _.Value != null)
                    .ToDictionary(
                    _ => _.Key,
                    _ => ((JObject)_.Value).Properties()
                        .ToDictionary(_ => _.Name, _ => _.Value.Values<string>().ToArray()));

                foreach (var (name, filteredItems) in groupedByAggregation)
                {
                    var highlightName = name.RemoveFromEnd(SearchQueryExtension.AggregateSuffix);
                    var ids = highlights
                        .Where(_ => (_.Value.TryGetValue(highlightName, out var highlight) || _.Value.TryGetValue(name, out highlight))
                            && ContainsHighlightFilteredValue(highlight, filteredItems))
                        .Select(_ => _.Key)
                        .ToArray();
                    var query = string.Join(IdFieldSeparator, ids);

                    results.Add(name, query);
                }

                return results;
            }

            private static bool ContainsHighlightFilteredValue(
                string[] highlightValues,
                IReadOnlyCollection<Property> filteredItems)
            {
                foreach (var filteredItem in filteredItems)
                {
                    if (highlightValues.Any(_ => _.Contains(filteredItem.Value)))
                    {
                        return true;
                    }
                }

                return false;
            }
        }
    }
}