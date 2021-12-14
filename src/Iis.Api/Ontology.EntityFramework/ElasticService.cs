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
using Iis.Elastic;
using Iis.DataModel.ChangeHistory;

namespace IIS.Core.Ontology.EntityFramework
{
    public class ElasticService : IElasticService
    {
        private const string ExclamationMark = "!";
        private const int MaxAggregationsCount = 20;

        private readonly IElasticManager _elasticManager;
        private readonly IElasticConfiguration _elasticConfiguration;
        private readonly INodeSaveService _nodeRepository;
        private readonly IElasticState _elasticState;
        private readonly IGroupedAggregationNameGenerator _groupedAggregationNameGenerator;
        private readonly IIisElasticField[] _historicalSearchFields =
        {
            new IisElasticField { Name = $"{ElasticSerializer.HistoricalPropertyName}.{nameof(ChangeHistoryDocument.OldValue) }" },
            new IisElasticField { Name = $"{ElasticSerializer.HistoricalPropertyName}.{nameof(ChangeHistoryDocument.NewValue)}" }
        };

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
            var ontologyFields = GetSearchFields(typeNames);
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
            var ontologyFields = GetSearchFields(typeNames);
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
            CancellationToken cancellationToken = default)
        {
            if (ShouldReturnAllEntities(filter))
            {
                var result = await GetAllSearchResultAsync(typeNames, filter, userId, cancellationToken);

                return result.ToOutputSearchResult();
            }

            var (multiSearchQuery, context) = PrepareMultiSearchQuery(typeNames, filter);
            var aggregations = await GetAggregationsQueryResultAsync(typeNames, filter, userId, !filter.IsExact && context.IsBaseQueryExact, cancellationToken);
            var multiSearchQueryString = multiSearchQuery
                .WithHighlights()
                .ToString();
            var searchResult = await _elasticManager.WithUserId(userId).SearchAsync(multiSearchQueryString, typeNames, cancellationToken);

            return searchResult.ToOutputSearchResult(aggregations);
        }

        public async Task<SearchEntitiesByConfiguredFieldsResult> FilterNodeCoordinatesAsync(
            IEnumerable<string> typeNames,
            ElasticFilter filter,
            CancellationToken cancellationToken = default)
        {
            var context = PrepareMultiElasticSearchContext(typeNames, filter);

            var query = new MultiSearchParamsQueryBuilder(context.ElasticMultiSearchParams.SearchParams)
                .WithPagination(context.ElasticMultiSearchParams.From, context.ElasticMultiSearchParams.Size)
                .WithLeniency(context.ElasticMultiSearchParams.IsLenient)
                .WithResultFields(context.ElasticMultiSearchParams.ResultFields)
                .BuildSearchQuery()
                .ToString();

            var searchResult = await _elasticManager.SearchAsync(query, typeNames, cancellationToken);

            return searchResult.ToOutputSearchResult();
        }

        public Task<int> CountEntitiesByConfiguredFieldsAsync(
            IEnumerable<string> typeNames,
            ElasticFilter filter,
            CancellationToken cancellationToken = default)
        {
            var context = PrepareMultiElasticSearchContext(typeNames, filter);
            var query = new MultiSearchParamsQueryBuilder(context.ElasticMultiSearchParams.SearchParams)
                .WithLeniency(context.ElasticMultiSearchParams.IsLenient)
                .BuildCountQuery()
                .ToString();

            return _elasticManager.CountAsync(query, typeNames, cancellationToken);
        }

        public Task<bool> PutNodeAsync(Guid id, CancellationToken ct = default)
        {
            return _nodeRepository.PutNodeAsync(id, ct);
        }

        public Task<bool> PutNodeAsync(Guid id, IEnumerable<string> fieldsToExtract, CancellationToken ct = default)
        {
            return _nodeRepository.PutNodeAsync(id, fieldsToExtract, ct);
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

        private (JObject Query, ISearchParamsContext SearchContext) PrepareMultiSearchQuery(
            IEnumerable<string> typeNames,
            ElasticFilter filter,
            bool applyFuzziness = false)
        {
            var searchParamsContext = PrepareMultiElasticSearchContext(typeNames, filter, applyFuzziness);
            var query = PrepareMultiSearchQuery(searchParamsContext.ElasticMultiSearchParams);

            return (query, searchParamsContext);
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

        private ISearchParamsContext PrepareMultiElasticSearchContext(
            IEnumerable<string> typeNames,
            ElasticFilter filter,
            bool applyFuzziness = false)
        {
            var searchFields = GetSearchFields(typeNames)
                .Concat(_historicalSearchFields)
                .ToList();
            var baseParameterQuery = filter.ToQueryString(applyFuzziness);
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

            return SearchParamsContext.CreateFrom(multiSearchParams);
        }

        private Task<IElasticSearchResult> GetAllSearchResultAsync(
            IEnumerable<string> typeNames,
            ElasticFilter filter,
            Guid userId,
            CancellationToken cancellationToken)
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

            return _elasticManager.WithUserId(userId).SearchAsync(query, typeNames, cancellationToken);
        }

        private async Task<Dictionary<string, AggregationItem>> GetAggregationsQueryResultAsync(
            IEnumerable<string> typeNames,
            ElasticFilter filter,
            Guid userId,
            bool applyFuzziness,
            CancellationToken cancellationToken)
        {
            var aggregatesElasticFilter = new ElasticFilter
            {
                CherryPickedItems = filter.CherryPickedItems,
                FilteredItems = filter.FilteredItems,
                Limit = filter.Limit,
                Offset = filter.Offset,
                Suggestion = filter.Suggestion
            };
            var (_, aggregatesContext) = PrepareMultiSearchQuery(typeNames, aggregatesElasticFilter, applyFuzziness);
            var multiSearchAggregationQuery = aggregatesContext.IsBaseQueryMatchAll
                ? new MatchAllQueryBuilder()
                    .WithPagination(filter.Offset, filter.Limit)
                    .WithResultFields(aggregatesContext.ElasticMultiSearchParams.ResultFields)
                    .BuildSearchQuery()
                : PrepareMultiSearchQuery(aggregatesContext.ElasticMultiSearchParams);
            var aggregationFields = aggregatesContext.ElasticMultiSearchParams.SearchParams.SelectMany(_ => _.Fields)
                .Where(_ => _.IsAggregated)
                .Select(_ => new AggregationField(AsAggregateName(_), _.Alias, AsAggregateName(_), _.Name))
                .ToArray();
            var batchQueries = GetBatchAggregateQueries(aggregationFields, multiSearchAggregationQuery, filter, aggregatesContext).ToArray();

            _elasticManager.WithUserId(userId);

            var aggregationQueryResults = await batchQueries.ForEachAsync(_ => _elasticManager.SearchAsync(_, typeNames, cancellationToken));
            var aggregations = aggregationQueryResults
                .SelectMany(_ => _.Aggregations)
                .ToDictionary(_ => _.Key, _ => _.Value);

            return ExtractSubAggregations(aggregations);
        }

        private IEnumerable<string> GetBatchAggregateQueries(
            IReadOnlyCollection<AggregationField> aggregationFields,
            JObject multiSearchAggregationQuery,
            ElasticFilter filter,
            ISearchParamsContext context)
        {
            var batchCount = aggregationFields.Count() / MaxAggregationsCount + 1;

            for (var batchIndex = 0; batchIndex < batchCount; batchIndex++)
            {
                var fieldsToAggregate = aggregationFields
                       .Skip(batchIndex * MaxAggregationsCount)
                       .Take(MaxAggregationsCount);

                yield return multiSearchAggregationQuery
                    .WithAggregation(fieldsToAggregate, filter, _groupedAggregationNameGenerator, context)
                    .ToString();
            }
        }

        private IReadOnlyList<IIisElasticField> GetSearchFields(IEnumerable<string> typeNames) => _elasticConfiguration
                .GetOntologyIncludedFields(typeNames.Where(_ => _elasticState.ObjectIndexes.Contains(_)))
                .ToList();

        private string AsAggregateName(IIisElasticField field) => $"{field.Name}{SearchQueryExtension.AggregateSuffix}";
    }
}