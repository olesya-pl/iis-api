using System;
using System.Linq;
using System.Collections.Generic;
using Iis.Elastic.SearchResult;
using Iis.Interfaces.Elastic;
using QuerySearchResult = Iis.Interfaces.Elastic.SearchResult;

namespace Iis.Elastic.SearchQueryExtensions
{
    public static class SearchResultsExtension
    {
        public static readonly Dictionary<string, AggregationItem> EmptyAggregation =
            new Dictionary<string, AggregationItem>();

        public static SearchEntitiesByConfiguredFieldsResult ToOutputSearchResult(
            this IElasticSearchResult elasticResult)
        {
            if (elasticResult is null) return null;

            var aggregations = elasticResult.Aggregations is null
                ? EmptyAggregation
                : elasticResult.Aggregations.Where(p => p.Value.Buckets.Any()).ToDictionary(p => p.Key, p => p.Value);

            return new SearchEntitiesByConfiguredFieldsResult
            {
                Count = elasticResult.Count,
                Entities = elasticResult.Items.Select(x => x.SearchResult).ToList(),
                Aggregations = aggregations
            };
        }

        public static SearchEntitiesByConfiguredFieldsResult ToOutputSearchResult(
            this IElasticSearchResult elasticResult, Dictionary<string, AggregationItem> aggregations)
        {
            if (elasticResult is null) return null;

            aggregations = aggregations is null
                ? EmptyAggregation
                : aggregations.Where(p => p.Value.Buckets.Any()).ToDictionary(p => p.Key, p => p.Value);

            return new SearchEntitiesByConfiguredFieldsResult
            {
                Count = elasticResult.Count,
                Entities = elasticResult.Items.Select(x => x.SearchResult).ToList(),
                Aggregations = aggregations
            };
        }

        public static QuerySearchResult ToSearchResult(this IElasticSearchResult elasticSearchResult)
        {
            var aggregations = elasticSearchResult.Aggregations is null
                ? EmptyAggregation
                : elasticSearchResult.Aggregations.Where(p => p.Value.Buckets.Any())
                    .ToDictionary(p => p.Key, p => p.Value);
            
            return new QuerySearchResult
            {
                Count = elasticSearchResult.Count,
                Items = elasticSearchResult.Items
                    .ToDictionary(k => new Guid(k.Identifier),
                        v => new SearchResultItem {Highlight = v.Higlight, SearchResult = v.SearchResult}),
                Aggregations = aggregations,
                ScrollId = elasticSearchResult.ScrollId
            };
        }
    }
}