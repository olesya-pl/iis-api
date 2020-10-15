using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Iis.Interfaces.Elastic
{
    public class AggregationBucket
    {
        public string Key { get; set; }
        public int DocCount { get; set; }
    }


    public class AggregationItem
    {
        public AggregationBucket[] Buckets { get; set; }
    }


    public interface IElasticSearchResult
    {
        int Count { get; }
        IEnumerable<IElasticSearchResultItem> Items { get; }
        Dictionary<string, AggregationItem> Aggregations { get; }
    }

    public interface IElasticSearchResultItem
    {
        string Identifier { get; set; }
        JToken Higlight { get; set; }
        JObject SearchResult { get; set; }        
    }
}