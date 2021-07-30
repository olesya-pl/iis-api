using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json;
using Iis.Interfaces.Elastic;
using Iis.Elastic.SearchQueryExtensions;
using Iis.Services.Contracts.Enums;
using Iis.Services.Contracts.Params;
using Iis.Services.Contracts.Interfaces;
using Iis.Services.Contracts.Interfaces.Elastic;
using Iis.Elastic;

namespace Iis.Services
{
    public class MaterialElasticService : IMaterialElasticService
    {
        private const string ExclamationMark = "!";
        private readonly IElasticManager _elasticManager;
        private readonly IElasticState _elasticState;
        private readonly IElasticResponseManagerFactory _elasticResponseManagerFactory;
        private readonly ElasticConfiguration _elasticConfiguration;
        private string[] MaterialIndexes = { "Materials" };
        private static IReadOnlyCollection<AggregationField> _aggregationsFieldList = new List<AggregationField>
        {
            new AggregationField("ProcessedStatus.Title", string.Empty, "ProcessedStatus.Title"),
            new AggregationField("Completeness.Title", string.Empty, "Completeness.Title"),
            new AggregationField("Importance.Title", string.Empty, "Importance.Title"),
            new AggregationField("SessionPriority.Title", string.Empty, "SessionPriority.Title"),
            new AggregationField("Reliability.Title", string.Empty, "Reliability.Title"),
            new AggregationField("Relevance.Title", string.Empty, "Relevance.Title"),
            new AggregationField("SourceReliability.Title", string.Empty, "SourceReliability.Title"),
            new AggregationField("Type.keyword", string.Empty, "Type.keyword"),
            new AggregationField("Source.keyword", string.Empty, "Source.keyword"),
        };

        public MaterialElasticService(IElasticManager elasticManager,
            IElasticState elasticState,
            IElasticResponseManagerFactory elasticResponseManagerFactory,
            ElasticConfiguration elasticConfiguration)
        {
            _elasticManager = elasticManager;
            _elasticState = elasticState;
            _elasticResponseManagerFactory = elasticResponseManagerFactory;
            _elasticConfiguration = elasticConfiguration;
        }

        public bool ShouldReturnNoEntities(string queryExpression)
        {
            return queryExpression.Trim() == ExclamationMark;
        }

        public async Task<SearchResult> SearchMaterialsByConfiguredFieldsAsync(Guid userId, SearchParams searchParams, CancellationToken ct = default)
        {
            var (from, size) = searchParams.Page.ToElasticPage();

            var queryString = SearchQueryExtension.CreateMaterialsQueryString(
                searchParams.Suggestion, 
                searchParams.FilteredItems, 
                searchParams.CherryPickedItems);

            var query = new ExactQueryBuilder()
                .WithPagination(from, size)
                .WithQueryString(queryString)
                .BuildSearchQuery()
                .WithAggregation(_aggregationsFieldList)
                .WithHighlights();

            if (searchParams.Sorting != null)
            {
                var (sortColumn, sortOrder) = MapSortingToElastic(searchParams.Sorting);
                query = query.SetupSorting(sortColumn, sortOrder);
            }

            var elasticResult = await _elasticManager
                .WithUserId(userId)
                .SearchAsync(query.ToString(Formatting.None), _elasticState.MaterialIndexes, ct);

            var searchResult = elasticResult.ToSearchResult();

            foreach (var item in searchResult.Items)
            {
                if (item.Value.Highlight is null) continue;

                item.Value.Highlight = await _elasticResponseManagerFactory.Create(SearchType.Material)
                 .GenerateHighlightsWithoutDublications(item.Value.SearchResult, item.Value.Highlight);
            }

            if (ItemsCountPossiblyExceedsMaxThreshold(searchResult))
            {
                var countQuery = new ExactQueryBuilder()
                    .WithQueryString(queryString)
                    .BuildCountQuery()
                    .ToString();
                searchResult.Count = await _elasticManager
                    .WithUserId(userId)
                    .CountAsync(countQuery, _elasticState.MaterialIndexes, ct);
            }

            return searchResult;
        }

        private static bool ItemsCountPossiblyExceedsMaxThreshold(SearchResult searchResult)
        {
            return searchResult.Count == ElasticConstants.MaxItemsCount;
        }

        public async Task<SearchResult> BeginSearchByScrollAsync(Guid userId, SearchParams searchParams, CancellationToken ct = default)
        {
            var (from, size) = searchParams.Page.ToElasticPage();

            var queryString = SearchQueryExtension.CreateMaterialsQueryString(
                searchParams.Suggestion,
                searchParams.FilteredItems,
                searchParams.CherryPickedItems);

            var scrollDuration = _elasticConfiguration.ScrollDurationMinutes == default(int)
                ? ElasticConstants.DefaultScrollDurationMinutes
                : _elasticConfiguration.ScrollDurationMinutes;            

            var query = new ExactQueryBuilder()
                .WithPagination(from, size)
                .WithQueryString(queryString)
                .BuildSearchQuery();

            var elasticResult = await _elasticManager
                .WithUserId(userId)
                .BeginSearchByScrollAsync(query.ToString(), TimeSpan.FromMinutes(scrollDuration), _elasticState.MaterialIndexes, ct);

            return elasticResult.ToSearchResult();
        }

        public async Task<SearchResult> SearchByScroll(Guid userId, string scrollId)
        {
            var scrollDuration = _elasticConfiguration.ScrollDurationMinutes == default(int)
                ? ElasticConstants.DefaultScrollDurationMinutes
                : _elasticConfiguration.ScrollDurationMinutes;

            var elasticResult = await _elasticManager
                .WithUserId(userId)
                .SearchByScrollAsync(scrollId, TimeSpan.FromMinutes(scrollDuration));
            return elasticResult.ToSearchResult();
        }

        public async Task<SearchResult> SearchMaterialsAsync(Guid userId,
            SearchParams searchParams, 
            IEnumerable<Guid> materialList, 
            CancellationToken ct = default)
        {
            var (from, size) = searchParams.Page.ToElasticPage();

            var queryBuilder = new BoolQueryBuilder()
                                .WithMust()
                                .WithPagination(from, size)
                                .WithDocumentList(materialList);

            if (!SearchQueryExtension.IsMatchAll(searchParams.Suggestion))
            {
                queryBuilder.WithExactQuery(searchParams.Suggestion);
            }

            var queryObj = queryBuilder
                            .BuildSearchQuery()
                            .WithHighlights();

            if (searchParams.Sorting != null)
            {
                var (sortColumn, sortOrder) = MapSortingToElastic(searchParams.Sorting);
                queryObj = queryObj.SetupSorting(sortColumn, sortOrder);
            }

            var query = queryObj
                .WithAggregation(_aggregationsFieldList)
                .ToString(Formatting.None);

            var elasticResult = await _elasticManager
                .WithUserId(userId)
                .SearchAsync(query, MaterialIndexes, ct);

            var searchResult = elasticResult.ToSearchResult();

            foreach (var item in searchResult.Items)
            {
                if (item.Value.Highlight is null) continue;

                item.Value.Highlight = await _elasticResponseManagerFactory.Create(SearchType.Material)
                 .GenerateHighlightsWithoutDublications(item.Value.SearchResult, item.Value.Highlight);
            }

            return searchResult;
        }

        public async Task<SearchResult> SearchMoreLikeThisAsync(Guid userId, SearchParams searchParams, CancellationToken ct = default)
        {
            var (from, size) = searchParams.Page.ToElasticPage();
            var (sortColumn, sortOrder) = MapSortingToElastic(searchParams.Sorting);

            var queryData = new MoreLikeThisQueryBuilder()
                        .WithPagination(from, size)
                        .WithMaterialId(searchParams.Suggestion)
                        .BuildSearchQuery()
                        .SetupSorting(sortColumn, sortOrder)
                        .ToString(Formatting.None);

            var searchResult = await _elasticManager
                .WithUserId(userId)
                .SearchAsync(queryData, _elasticState.MaterialIndexes, ct);

            return searchResult.ToSearchResult();
        }

        public async Task<SearchResult> SearchByImageVector(Guid userId, IReadOnlyCollection<decimal[]> imageVectorList, PaginationParams page, CancellationToken ct = default)
        {
            var (from, size) = page.ToElasticPage();

            var query = new SearchByImageQueryBuilder(imageVectorList)
                .WithPagination(from, size)
                .BuildSearchQuery()
                .WithAggregation(_aggregationsFieldList);

            var searchResult = await _elasticManager
                .WithUserId(userId)
                .SearchAsync(query.ToString(), _elasticState.MaterialIndexes, ct);

            return searchResult.ToSearchResult();
        }

        public Task<int> CountMaterialsByConfiguredFieldsAsync(Guid userId, SearchParams searchParams, CancellationToken ct = default)
        {
            var queryString = SearchQueryExtension.CreateMaterialsQueryString(
                searchParams.Suggestion,
                searchParams.FilteredItems,
                searchParams.CherryPickedItems);

            var pagination = searchParams.Page.ToEFPage();
            
            var exactQueryBuilder = new ExactQueryBuilder()
                .WithQueryString(queryString)
                .WithLeniency(true)
                .WithPagination(pagination.Skip, pagination.Take)
                .BuildCountQuery();

            return _elasticManager
                .WithUserId(userId)
                .CountAsync(exactQueryBuilder.ToString(), _elasticState.MaterialIndexes, ct);
        }

        private static (string SortColumn, string SortOrder) MapSortingToElastic(SortingParams sorting)
        {
            return sorting.ColumnName switch
            {
                "createdDate" => ("CreatedDate", sorting.Order),
                "type" => ("Type.keyword", sorting.Order),
                "source" => ("Source.keyword", sorting.Order),
                "processedStatus" => ("ProcessedStatus.OrderNumber", sorting.Order),
                "sessionPriority" => ("SessionPriority.OrderNumber", sorting.Order),
                "importance" => ("Importance.OrderNumber", sorting.Order),
                "nodes" => ("ObjectsOfStudyCount", sorting.Order),
                _ => (null, null)
            };
        }
    }
}