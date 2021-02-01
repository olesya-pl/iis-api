using System.Collections.Generic;
using Iis.Interfaces.Elastic;

namespace Iis.Elastic.SearchResult
{
    public class ElasticSearchResult : IElasticSearchResult
    {
        public int Count { get; set; }
        public IEnumerable<IElasticSearchResultItem> Items { get; set; }
        public Dictionary<string, AggregationItem> Aggregations { get; set; }
        public string ScrollId { get; set; }
    }

}
