using System.Collections.Generic;
using System.Linq;
using Elasticsearch.Net;
using Iis.Interfaces.Elastic;
using Iis.Interfaces.Ontology.Schema;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace Iis.Elastic.SearchResult
{
    internal class SearchResultExtractor
    {
        private readonly IFieldToAliasMapper _fieldToAliasMapper;
        public SearchResultExtractor(IFieldToAliasMapper fieldToAliasMapper)
        {
            _fieldToAliasMapper = fieldToAliasMapper;
        }

        public SearchResultExtractor()
        {

        }

        public IElasticSearchResult GetFromResponse(StringResponse response)
        {
            var json = JObject.Parse(response.Body);
            List<ElasticSearchResultItem> items = null;
            const string NODE_TYPE_NAME = "NodeTypeName";
            const string HIGHLIGHT = "highlight";

            var hits = json["hits"]?["hits"];
            if (hits != null)
            {
                items = hits.Select(hit => {
                    var resultItem = new ElasticSearchResultItem
                    {
                        Identifier = hit["_id"].ToString(),
                        Higlight = hit[HIGHLIGHT],
                        SearchResult = hit["_source"] as JObject
                    };
                    var nodeTypeName = resultItem.SearchResult[NODE_TYPE_NAME]?.ToString();
                    resultItem.SearchResult[HIGHLIGHT] = RemoveFieldsDuplicatedByAlias(resultItem.Higlight, nodeTypeName);
                    if (resultItem.SearchResult[NODE_TYPE_NAME] != null)
                    {
                        resultItem.SearchResult["__typename"] = $"Entity{resultItem.SearchResult[NODE_TYPE_NAME]}";
                    }
                    return resultItem;
                }).ToList();                
            }
            var total = json["hits"]?["total"]?["value"];
            var res = new ElasticSearchResult
            {
                Count = (int?)total ?? 0,
                Items = items ?? new List<ElasticSearchResultItem>()
            };
            if (json.ContainsKey("aggregations"))
            {
                res.Aggregations = json["aggregations"].ToObject<Dictionary<string, AggregationItem>>(new JsonSerializer
                {
                    ContractResolver = new DefaultContractResolver
                    {
                        NamingStrategy = new SnakeCaseNamingStrategy()
                    }
                });
            }
            if (json.ContainsKey("_scroll_id"))
            {
                res.ScrollId = json["_scroll_id"].ToString();
            }
            return res;
        }

        private JObject RemoveFieldsDuplicatedByAlias(JToken highlight, string nodeTypeName)
        {
            if (highlight == null || string.IsNullOrEmpty(nodeTypeName)) return null;

            var result = new JObject();
            foreach (JProperty child in highlight.Children())
            {
                var fullName = $"{nodeTypeName}.{child.Name}";
                var alias = _fieldToAliasMapper?.GetAlias(fullName);
                if (alias == null)
                {
                    result[child.Name] = child.Value;
                }
            }

            return result;
        }
    }
}
