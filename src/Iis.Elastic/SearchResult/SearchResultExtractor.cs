using System.Collections.Generic;
using System.Linq;
using Elasticsearch.Net;
using Iis.Elastic.Converters;
using Iis.Elastic.SearchQueryExtensions;
using Iis.Interfaces.Elastic;
using Iis.Interfaces.Ontology.Schema;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace Iis.Elastic.SearchResult
{
    internal class SearchResultExtractor
    {
        private const string NODE_TYPE_NAME = "NodeTypeName";
        private const string HIGHLIGHT = "highlight";

        private readonly IFieldToAliasMapper _fieldToAliasMapper;

        public SearchResultExtractor()
        {
        }

        public SearchResultExtractor(IFieldToAliasMapper fieldToAliasMapper)
        {
            _fieldToAliasMapper = fieldToAliasMapper;
        }

        public IElasticSearchResult GetFromResponse(StringResponse response)
        {
            if (!response.Success)
            {
                throw response.OriginalException;
            }

            var json = JObject.Parse(response.Body);
            var result = new ElasticSearchResult
            {
                Count = ReadTotalCount(json),
                Items = ReadSearchResultItems(json)
            };

            PopulateAggregations(json, result);
            PopulateScrollId(json, result);

            return UnwrapGroupedAggregations(result);
        }

        private int ReadTotalCount(JObject response)
        {
            var token = response["hits"]?["total"]?["value"];

            return (int?)token ?? 0;
        }

        private void PopulateAggregations(JObject response, ElasticSearchResult result)
        {
            if (!response.ContainsKey("aggregations")) return;

            var contractResolver = new DefaultContractResolver
            {
                NamingStrategy = new SnakeCaseNamingStrategy()
            };
            var serializer = new JsonSerializer()
            {
                ContractResolver = contractResolver
            };

            result.Aggregations = response["aggregations"]
                .ToObject<Dictionary<string, SerializableAggregationItem>>(serializer)
                .ToDictionary(_ => _.Key, _ => _.Value as AggregationItem);
        }

        private List<ElasticSearchResultItem> ReadSearchResultItems(JObject response)
        {
            var hits = response["hits"]?["hits"];
            if (hits == null) return new List<ElasticSearchResultItem>();

            return hits.Select(hit =>
            {
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

        private void PopulateScrollId(JObject response, ElasticSearchResult result)
        {
            if (response.ContainsKey("_scroll_id"))
                result.ScrollId = response["_scroll_id"].ToString();
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

        private IElasticSearchResult UnwrapGroupedAggregations(ElasticSearchResult elasticSearchResult)
        {
            if (elasticSearchResult.Aggregations is null
                || elasticSearchResult.Aggregations.Count == 0)
                return elasticSearchResult;

            var unwrapedAggregations = UnwrapGroupedAggregations(elasticSearchResult.Aggregations);

            elasticSearchResult.Aggregations = new Dictionary<string, AggregationItem>(unwrapedAggregations);

            return elasticSearchResult;
        }

        private IEnumerable<KeyValuePair<string, AggregationItem>> UnwrapGroupedAggregations(Dictionary<string, AggregationItem> aggregations)
        {
            foreach (var pair in aggregations)
            {
                if (pair.Key.IsGroupedAggregateName())
                {
                    foreach (var aggregationItem in pair.Value.GroupedSubAggs)
                    {
                        yield return aggregationItem;
                    }

                    continue;
                }

                yield return pair;
            }
        }
    }

    [JsonConverter(typeof(AggregationItemConverter))]
    internal class SerializableAggregationItem : AggregationItem
    {
    }
}