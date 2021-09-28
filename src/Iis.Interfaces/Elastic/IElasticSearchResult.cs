using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Iis.Interfaces.Elastic
{
    //TODO: надо с этим файлом что-то делать
    public class AggregationBucket
    {
        public string Key { get; set; }
        public int DocCount { get; set; }
    }


    public class AggregationItem
    {
        public AggregationBucket[] Buckets { get; set; }

        public AggregationItem SubAggs { get; set; }

        public Dictionary<string, AggregationItem> GroupedSubAggs { get; set; }
    }


    public interface IElasticSearchResult
    {
        int Count { get; }
        IEnumerable<IElasticSearchResultItem> Items { get; }
        Dictionary<string, AggregationItem> Aggregations { get; }
        string ScrollId { get; }
    }

    public interface IElasticSearchResultItem
    {
        string Identifier { get; set; }
        JToken Higlight { get; set; }
        JObject SearchResult { get; set; }        
    }

    public class SearchResult
    {
        public Dictionary<Guid, SearchResultItem> Items { get; set; }
        public Dictionary<string, AggregationItem> Aggregations { get; set; }
        public int Count { get; set; }
        public string ScrollId { get; set; }
    }

    public class SearchResultItem
    {
        public JToken Highlight { get; set; }
        public JObject SearchResult { get; set; }
    }
}