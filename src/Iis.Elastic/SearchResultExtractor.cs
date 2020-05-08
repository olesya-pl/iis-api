using System.Collections.Generic;
using Elasticsearch.Net;
using Iis.Interfaces.Elastic;
using Newtonsoft.Json.Linq;

namespace Iis.Elastic
{
    public class SearchResultExtractor
    {
        public IElasticSearchResult GetFromResponse(StringResponse response)
        {
            var json = JObject.Parse(response.Body);
            var items = new List<ElasticSearchResultItem>();

            var hits = json["hits"]?["hits"];
            if (hits != null)
            {
                foreach (var hit in hits)
                {
                    var resultItem = new ElasticSearchResultItem
                    {
                        Identifier = hit["_id"].ToString(),
                        Higlight = hit["highlight"],
                        SearchResult = hit["_source"] as JObject
                    };
                    resultItem.SearchResult["highlight"] = resultItem.Higlight;
                    if (resultItem.SearchResult["NodeTypeName"] != null)
                    {
                        resultItem.SearchResult["__typename"] = $"Entity{resultItem.SearchResult["NodeTypeName"]}";
                    }
                    items.Add(resultItem);

                }
            }
            var total = json["hits"]?["total"]?["value"];
            return new ElasticSearchResult
            {
                Count = (int?)total ?? 0,
                Items = items
            };
        }
    }
}
