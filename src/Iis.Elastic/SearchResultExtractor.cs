using System.Collections.Generic;
using Elasticsearch.Net;
using Iis.Interfaces.Elastic;
using Iis.Interfaces.Ontology.Schema;
using Newtonsoft.Json.Linq;

namespace Iis.Elastic
{
    public class SearchResultExtractor
    {
        IFieldToAliasMapper _fieldToAliasMapper;
        public SearchResultExtractor(IFieldToAliasMapper fieldToAliasMapper)
        {
            _fieldToAliasMapper = fieldToAliasMapper;
        }

        public IElasticSearchResult GetFromResponse(StringResponse response)
        {
            var json = JObject.Parse(response.Body);
            var items = new List<ElasticSearchResultItem>();
            const string NODE_TYPE_NAME = "NodeTypeName";
            const string HIGHLIGHT = "highlight";

            var hits = json["hits"]?["hits"];
            if (hits != null)
            {
                foreach (var hit in hits)
                {
                    var resultItem = new ElasticSearchResultItem
                    {
                        Identifier = hit["_id"].ToString(),
                        Higlight = hit[HIGHLIGHT],
                        SearchResult = hit["_source"] as JObject
                    };
                    var nodeTypeName = resultItem.SearchResult[NODE_TYPE_NAME].ToString();
                    resultItem.SearchResult[HIGHLIGHT] = RemoveFieldsDuplicatedByAlias(resultItem.Higlight, nodeTypeName);
                    if (resultItem.SearchResult[NODE_TYPE_NAME] != null)
                    {
                        resultItem.SearchResult["__typename"] = $"Entity{resultItem.SearchResult[NODE_TYPE_NAME]}";
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

        private JObject RemoveFieldsDuplicatedByAlias(JToken highlight, string nodeTypeName)
        {
            if (highlight == null) return null;

            var result = new JObject();
            foreach (JProperty child in highlight.Children())
            {
                var fullName = $"{nodeTypeName}.{child.Name}";
                var alias = _fieldToAliasMapper.GetAlias(fullName);
                if (alias == null)
                {
                    result[child.Name] = child.Value;
                }
            }

            return result;
        }
    }
}
