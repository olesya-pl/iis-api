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

namespace Iis.Services
{
    public class MaterialElasticService : IMaterialElasticService
    {
        private readonly IElasticManager _elasticManager;
        private readonly IElasticState _elasticState;
        private readonly IElasticResponseManagerFactory _elasticResponseManagerFactory;

        private string[] MaterialIndexes = { "Materials" };

        private static List<AggregateFieldMap> _aggregationsMap = new List<AggregateFieldMap>
        {
            new AggregateFieldMap("ProcessedStatus", "ProcessedStatus.Title"),
            new AggregateFieldMap("Completeness", "Completeness.Title"),
            new AggregateFieldMap("Importance", "Importance.Title"),
            new AggregateFieldMap("SessionPriority", "SessionPriority.Title"),
            new AggregateFieldMap("Reliability", "Reliability.Title"),
            new AggregateFieldMap("Relevance", "Relevance.Title"),
            new AggregateFieldMap("SourceReliability", "SourceReliability.Title"),
            new AggregateFieldMap("Type", "Type.keyword"),
            new AggregateFieldMap("Source", "Source.keyword"),
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

            var (sortColumn, sortOrder) = MapSortingToElastic(searchParams.Sorting);

            var (from, size) = searchParams.Page.ToElasticPage();
                      
            var queryString = noSuggestion ? "ParentId:NULL" : $"{searchParams.Suggestion} AND ParentId:NULL";

                        
            
            var builder = new ExactQueryBuilder();
            var query = builder.WithPagination(from, size)
                .WithQueryString(queryString)
                .Build()
                .WithExactAggregationNames(_aggregationsMap)
                .WithHighlights()
                .SetupSorting(sortColumn, sortOrder);

            var elasticResult = await _elasticManager.SearchAsync(query.ToString(), _elasticState.MaterialIndexes, ct);
            
            var searchResult = MapToSearchResult(elasticResult);
            
            foreach (var item in searchResult.Items)
            {
                if (item.Value.Highlight is null) continue;

                item.Value.Highlight = await _elasticResponseManagerFactory.Create(SearchType.Material)
                 .GenerateHighlightsWithoutDublications(item.Value.SearchResult, item.Value.Highlight);
            }

            return searchResult;
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

            var (sortColumn, sortOrder) = MapSortingToElastic(searchParams.Sorting);

            var query = queryBuilder
                            .Build()
                            .WithHighlights()
                            .SetupSorting(sortColumn, sortOrder)
                            .ToString(Formatting.None);

            var elasticResult = await _elasticManager.SearchAsync(query, MaterialIndexes, ct);

            var searchResult = MapToSearchResult(elasticResult);

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

            return MapToSearchResult(searchResult);
        }

        public async Task<SearchResult> SearchByImageVector(decimal[] imageVector, PaginationParams page, CancellationToken ct = default)
        {
            var (from, size) = page.ToElasticPage();

            var query = new SearchByImageQueryBuilder(imageVector)
                .WithPagination(from, size)
                .Build()
                .WithExactAggregationNames(_aggregationsMap);

            var searchResult = await _elasticManager.SearchAsync(query.ToString(), _elasticState.MaterialIndexes, ct);

            return MapToSearchResult(searchResult);
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

        private static SearchResult MapToSearchResult(IElasticSearchResult elasticSearchResult)
        {
            return new SearchResult
            {
                Count = elasticSearchResult.Count,
                Items = elasticSearchResult.Items
                    .ToDictionary(k => new Guid(k.Identifier),
                    v => new SearchResultItem { Highlight = v.Higlight, SearchResult = v.SearchResult }),
                Aggregations = elasticSearchResult.Aggregations is null
                ? new Dictionary<string, AggregationItem>()
                : elasticSearchResult.Aggregations.Where(p => p.Value.Buckets.Any()).ToDictionary(p => p.Key, p => p.Value)
            };
        }
    }
}