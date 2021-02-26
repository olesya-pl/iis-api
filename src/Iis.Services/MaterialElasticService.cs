using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json;

using Iis.Domain.Elastic;
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
        private readonly IElasticManager _elasticManager;
        private readonly IElasticState _elasticState;
        private readonly IElasticResponseManagerFactory _elasticResponseManagerFactory;
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
            IElasticResponseManagerFactory elasticResponseManagerFactory
            )
        {
            _elasticManager = elasticManager;
            _elasticState = elasticState;
            _elasticResponseManagerFactory = elasticResponseManagerFactory;
        }

        public async Task<SearchResult> SearchMaterialsByConfiguredFieldsAsync(SearchParams searchParams, CancellationToken ct = default)
        {
            var noSuggestion = string.IsNullOrEmpty(searchParams.Suggestion);

            var (from, size) = searchParams.Page.ToElasticPage();

            var queryString = noSuggestion ? "ParentId:NULL" : $"{searchParams.Suggestion} AND ParentId:NULL";

            var query = new ExactQueryBuilder()
                .WithPagination(from, size)
                .WithQueryString(queryString)
                .Build()
                .WithAggregation(_aggregationsFieldList)
                .WithHighlights();

            if (searchParams.Sorting != null)
            {
                var (sortColumn, sortOrder) = MapSortingToElastic(searchParams.Sorting);
                query = query.SetupSorting(sortColumn, sortOrder);
            }

            var elasticResult = await _elasticManager.SearchAsync(query.ToString(), _elasticState.MaterialIndexes, ct);

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
                searchResult.Count = await _elasticManager.CountAsync(countQuery, _elasticState.MaterialIndexes, ct);

            }

            return searchResult;
        }

        private static bool ItemsCountPossiblyExceedsMaxThreshold(SearchResult searchResult)
        {
            return searchResult.Count == ElasticConstants.MaxItemsCount;
        }

        public async Task<SearchResult> BeginSearchByScrollAsync(SearchParams searchParams, TimeSpan scrollDuration = default, CancellationToken ct = default)
        {
            var noSuggestion = string.IsNullOrEmpty(searchParams.Suggestion);

            var (from, size) = searchParams.Page.ToElasticPage();

            var queryString = noSuggestion ? "ParentId:NULL" : $"{searchParams.Suggestion} AND ParentId:NULL";

            var query = new ExactQueryBuilder()
                .WithPagination(from, size)
                .WithQueryString(queryString)
                .Build();

            var elasticResult = await _elasticManager.BeginSearchByScrollAsync(query.ToString(), scrollDuration, _elasticState.MaterialIndexes, ct);

            return elasticResult.ToSearchResult();
        }

        public async Task<SearchResult> SearchByScroll(string scrollId, TimeSpan scrollDuration)
        {
            IElasticSearchResult elasticResult = await _elasticManager.SearchByScrollAsync(scrollId, scrollDuration);
            return elasticResult.ToSearchResult();
        }

        public async Task<SearchResult> SearchMaterialsAsync(SearchParams searchParams, 
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
                            .Build()
                            .WithHighlights();

            if (searchParams.Sorting != null)
            {
                var (sortColumn, sortOrder) = MapSortingToElastic(searchParams.Sorting);
                queryObj = queryObj.SetupSorting(sortColumn, sortOrder);
            }

            var query = queryObj
                .WithAggregation(_aggregationsFieldList)
                .ToString(Formatting.None);

            var elasticResult = await _elasticManager.SearchAsync(query, MaterialIndexes, ct);

            var searchResult = elasticResult.ToSearchResult();

            foreach (var item in searchResult.Items)
            {
                if (item.Value.Highlight is null) continue;

                item.Value.Highlight = await _elasticResponseManagerFactory.Create(SearchType.Material)
                 .GenerateHighlightsWithoutDublications(item.Value.SearchResult, item.Value.Highlight);
            }

            return searchResult;
        }

        public async Task<SearchResult> SearchMoreLikeThisAsync(SearchParams searchParams, CancellationToken ct = default)
        {
            var (from, size) = searchParams.Page.ToElasticPage();
            var (sortColumn, sortOrder) = MapSortingToElastic(searchParams.Sorting);

            var queryData = new MoreLikeThisQueryBuilder()
                        .WithPagination(from, size)
                        .WithMaterialId(searchParams.Suggestion)
                        .Build()
                        .SetupSorting(sortColumn, sortOrder)
                        .ToString(Formatting.None);

            var searchResult = await _elasticManager.SearchAsync(queryData, _elasticState.MaterialIndexes, ct);

            return searchResult.ToSearchResult();
        }

        public async Task<SearchResult> SearchByImageVector(decimal[] imageVector, PaginationParams page, CancellationToken ct = default)
        {
            var (from, size) = page.ToElasticPage();

            var query = new SearchByImageQueryBuilder(imageVector)
                .WithPagination(from, size)
                .Build()
                .WithAggregation(_aggregationsFieldList);

            var searchResult = await _elasticManager.SearchAsync(query.ToString(), _elasticState.MaterialIndexes, ct);

            return searchResult.ToSearchResult();
        }

        public Task<int> CountMaterialsByConfiguredFieldsAsync(SearchParams searchParams, CancellationToken ct = default)
        {
            var elasticSearchParams = new IisElasticSearchParams
            {
                BaseIndexNames = MaterialIndexes.ToList(),
                Query = string.IsNullOrEmpty(searchParams.Suggestion) ? "ParentId:NULL" : $"{searchParams.Suggestion} AND ParentId:NULL"
            };

            return _elasticManager.CountAsync(elasticSearchParams, ct);
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
                "nodes" => ("NodesCount", sorting.Order),
                _ => (null, null)
            };
        }        
    }
}