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
        private static readonly Dictionary<string, AggregationItem> EmptyAggregation =
            new Dictionary<string, AggregationItem>();

        private static Dictionary<string, AggregationItem> RemoveOnlyOtherValuesAggregations(
            Dictionary<string, AggregationItem> aggregations)
        {
            if (aggregations == EmptyAggregation) return aggregations;

            var noValueKeyCollection = aggregations
                .ToArray()
                .Where(e => e.Value.Buckets.Count() == 1 &&
                            e.Value.Buckets.Single().Key == SearchQueryExtension.MissingValueKey)
                .Select(e => e.Key)
                .ToArray();

            foreach (var key in noValueKeyCollection)
            {
                aggregations.Remove(key);
            }

            return aggregations;
        }

        public static SearchEntitiesByConfiguredFieldsResult ToOutputSearchResult(
            this IElasticSearchResult elasticResult)
        {
            if (elasticResult is null) return null;

            var aggregations = elasticResult.Aggregations is null
                ? EmptyAggregation
                : elasticResult.Aggregations.Where(p => p.Value.Buckets.Any()).ToDictionary(p => p.Key, p => p.Value);

            aggregations = RemoveOnlyOtherValuesAggregations(aggregations);

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

            aggregations = RemoveOnlyOtherValuesAggregations(aggregations);

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

            aggregations = RemoveOnlyOtherValuesAggregations(aggregations);

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