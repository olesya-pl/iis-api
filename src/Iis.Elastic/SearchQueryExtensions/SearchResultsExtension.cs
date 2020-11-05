using System.Linq;
using System.Collections.Generic;

using Iis.Interfaces.Elastic;

namespace Iis.Elastic.SearchQueryExtensions
{
    public static class SearchResultsExtension
    {
        public static SearchEntitiesByConfiguredFieldsResult ToOutputSearchResult(this IElasticSearchResult elasticResult)
        {
            if(elasticResult is null) return null;

            var aggregations = elasticResult.Aggregations is null
            ? new Dictionary<string, AggregationItem>()
            : elasticResult.Aggregations.Where(p => p.Value.Buckets.Any()).ToDictionary(p => p.Key, p => p.Value);

            return new SearchEntitiesByConfiguredFieldsResult
            {
                Count = elasticResult.Count,
                Entities = elasticResult.Items.Select(x => x.SearchResult).ToList(),
                Aggregations = aggregations
            };
        }
    }
}