using System.Collections.Generic;
using Iis.Interfaces.Elastic;
using Newtonsoft.Json.Linq;

namespace Iis.Elastic
{
    public class ElasticSearchResult : IElasticSearchResult
    {
        public int Count { get; set; }
        public IEnumerable<IElasticSearchResultItem> Items { get; set; }

        public Dictionary<string, AggregationItem> Aggregations { get; set; }
    }

    public class ElasticSearchResultItem : IElasticSearchResultItem
    {
        public string Identifier { get; set; }
        public JToken Higlight { get; set; }
        public JObject SearchResult { get; set; }
    }

}
