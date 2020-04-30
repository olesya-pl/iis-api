using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Elasticsearch.Net;
using Newtonsoft.Json.Linq;

using Iis.Interfaces.Elastic;
using Iis.Interfaces.Ontology;

namespace Iis.Elastic
{
    public class ElasticManager: IElasticManager
    {
        private const string EscapeSymbolsPattern = "^\"~:(){}[]\\/";
        private const string RemoveSymbolsPattern = "№";
        ElasticLowLevelClient _lowLevelClient;
        ElasticConfiguration _configuration;

        public ElasticManager(ElasticConfiguration configuration)
        {
            _configuration = configuration;

            var connectionPool = new SniffingConnectionPool(new[] { new Uri(_configuration.Uri) });

            var config = new ConnectionConfiguration(connectionPool);

            _lowLevelClient = new ElasticLowLevelClient(config);
        }

        public async Task<bool> PutDocumentAsync(string indexName, string documentId, string jsonDocument, CancellationToken cancellationToken = default)
        {
            if(string.IsNullOrWhiteSpace(indexName) || string.IsNullOrWhiteSpace(documentId) || string.IsNullOrWhiteSpace(jsonDocument)) return false;

            var indexUrl = $"{GetRealIndexName(indexName)}/_doc/{documentId}";

            PostData postData = jsonDocument;

            var response = await _lowLevelClient.DoRequestAsync<StringResponse>(HttpMethod.PUT, indexUrl, cancellationToken, postData);

            return response.Success;
        }

        public async Task<bool> DeleteDocumentAsync(string indexName, string documentId)
        {
            var searchResponse = await _lowLevelClient.DeleteAsync<StringResponse>(GetRealIndexName(indexName), documentId);

            return searchResponse.Success;
        }

        public async Task<IElasticSearchResult> Search(IIisElasticSearchParams searchParams, CancellationToken cancellationToken = default)
        {
            var jsonString = GetSearchJson(searchParams);
            var path = searchParams.BaseIndexNames.Count == 0 ?
                "_search" :
                $"{GetRealIndexNames(searchParams.BaseIndexNames)}/_search";

            var response = await GetAsync(path, jsonString, cancellationToken);
            return GetSearchResultFromResponse(response);
        }

        public async Task<IElasticSearchResult> GetDocumentIdListFromIndexAsync(string indexName)
        {
            if (indexName == null) throw new ArgumentNullException(nameof(indexName));

            var searchResponse = await _lowLevelClient.SearchAsync<StringResponse>(GetRealIndexName(indexName), PostData.Serializable(new
            {
                query = new
                {
                    match_all = new object()
                },
                stored_fields = Array.Empty<object>()
            }));

            if (!searchResponse.Success)
            {
                return new ElasticSearchResult();
            }

            return GetSearchResultFromResponse(searchResponse);
        }

        public async Task<string> GetDocumentByIdAsync(string indexName, string documentId, string[] documentFields)
        {
            var searchResponse = await _lowLevelClient.SearchAsync<StringResponse>(GetRealIndexName(indexName), PostData.Serializable(new
            {
                query = new
                {
                    match = new
                    {
                        _id = documentId
                    }
                },
                _source = documentFields
            }));

            if (!searchResponse.Success)
            {
                return string.Empty;
            }

            return searchResponse.Body;
        }

        public async Task CreateIndexesAsync(IEnumerable<string> indexNames, CancellationToken token)
        {
            foreach (string indexName in indexNames)
            {
                if (!await IndexExistsAsync(indexName, token))
                {
                    await CreateIndexAsync(indexName, token);
                }
            }
        }

        public async Task<bool> DeleteIndexesAsync(IEnumerable<string> indexNames, CancellationToken cancellationToken = default)
        {
            var path = $"{GetRealIndexNames(indexNames)}";

            var response = await DoRequestAsync(HttpMethod.DELETE, path, string.Empty, cancellationToken);

            return response.Success;
        }

        public async Task<bool> DeleteIndexAsync(string indexName, CancellationToken cancellationToken = default)
        {
            var indexUrl = $"{GetRealIndexName(indexName)}";

            var response = await DoRequestAsync(HttpMethod.DELETE, indexUrl, string.Empty, cancellationToken);

            return response.Success;
        }
        private async Task<bool> IndexExistsAsync(string indexName, CancellationToken token)
        {
            var searchResponse = await _lowLevelClient.SearchAsync<StringResponse>(GetRealIndexName(indexName), PostData.Serializable(new
            {
                size = 1,
                query = new
                {
                    match_all = new object()
                },
                stored_fields = Array.Empty<object>()
            }),
            ctx:token);

            return searchResponse.Success || searchResponse.HttpStatusCode != 404;
        }

        private async Task<bool> CreateIndexAsync(string indexName, CancellationToken token)
        {
            StringResponse response = await _lowLevelClient.DoRequestAsync<StringResponse>(HttpMethod.PUT, GetRealIndexName(indexName), token);
            return response.Success;
        }

        private IElasticSearchResult GetSearchResultFromResponse(StringResponse response)
        {
            var json = JObject.Parse(response.Body);
            var items = new List<ElasticSearchResultItem>();

            var hits = json["hits"]["hits"];
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
            var total = json["hits"]["total"]["value"];
            return new ElasticSearchResult
            {
                Count = (int)total,
                Items = items
            };
        }

        private string GetSearchJson(IIisElasticSearchParams searchParams)
        {
            var json = new JObject();
            json["_source"] = new JArray(searchParams.ResultFields);
            json["from"] = searchParams.From;
            json["size"] = searchParams.Size;
            json["query"] = new JObject();

            PrepareHighlights(json);

            if (searchParams.SearchFields?.Any() == true)
            {
                PopulateFallbackIntoQuery(searchParams, json);
            }
            else
            {
                PrepareFallbackQuery(searchParams, json);
            }

            return json.ToString();
        }

        private void PopulateFallbackIntoQuery(IIisElasticSearchParams searchParams, JObject json)
        {
            json["query"]["bool"] = new JObject();
            var columns = new JArray();

            json["query"]["bool"]["should"] = columns;
            foreach (var searchFieldGroup in searchParams.SearchFields.GroupBy(p => new { p.Fuzziness, p.Boost }))
            {
                var query = new JObject();
                var queryString = new JObject();
                queryString["query"] = ApplyFuzzinessOperator(
                    EscapeElasticSpecificSymbols(
                        RemoveSymbols(searchParams.Query, RemoveSymbolsPattern),
                    EscapeSymbolsPattern));
                queryString["fuzziness"] = searchFieldGroup.Key.Fuzziness;
                queryString["boost"] = searchFieldGroup.Key.Boost;
                queryString["lenient"] = searchParams.IsLenient;
                queryString["fields"] = new JArray(searchFieldGroup.Select(p => p.Name));
                query["query_string"] = queryString;
                columns.Add(query);
            }
        }

        private void PrepareFallbackQuery(IIisElasticSearchParams searchParams, JObject json)
        {
            var queryString = new JObject();
            queryString["query"] = EscapeElasticSpecificSymbols(
                RemoveSymbols(searchParams.Query, RemoveSymbolsPattern), EscapeSymbolsPattern);
            queryString["fields"] = new JArray("*");
            queryString["lenient"] = searchParams.IsLenient;
            json["query"]["query_string"] = queryString;
        }

        private static void PrepareHighlights(JObject json)
        {
            json["highlight"] = new JObject();
            json["highlight"]["fields"] = new JObject();
            json["highlight"]["fields"]["*"] = JObject.Parse("{\"type\" : \"plain\"}");
        }

        private string GetRealIndexName(string baseIndexName)
        {
            return $"{_configuration.IndexPreffix}{baseIndexName}".ToLower();
        }

        private string GetRealIndexNames(IEnumerable<string> baseIndexNames)
        {
            return string.Join(',', baseIndexNames.Select(name => GetRealIndexName(name)));
        }

        private string ApplyFuzzinessOperator(string input)
        {
            return $"{input}~";
        }

        private string EscapeElasticSpecificSymbols(string input, string escapePattern)
        {
            if (string.IsNullOrWhiteSpace(input)) return input;

            if (string.IsNullOrWhiteSpace(escapePattern)) throw new ArgumentNullException(nameof(escapePattern));

            var builder = new StringBuilder();

            foreach (var ch in input)
            {
                if (escapePattern.Contains(ch))
                {
                    builder.Append('\\');
                }

                builder.Append(ch);
            }

            return builder.ToString();
        }

        private string RemoveSymbols(string input, string removeSymbols)
        {
            if(string.IsNullOrWhiteSpace(input)) return input;

            if(string.IsNullOrWhiteSpace(removeSymbols)) throw new ArgumentNullException(nameof(removeSymbols));

            var builder = new StringBuilder();

            foreach (var ch in input)
            {
                if(removeSymbols.Contains(ch)) continue;

                builder.Append(ch);
            }
            return builder.ToString();
        }
        private async Task<StringResponse> DoRequestAsync(HttpMethod httpMethod, string path, string data, CancellationToken cancellationToken)
        {
            PostData postData = data;

            return await _lowLevelClient.DoRequestAsync<StringResponse>(httpMethod, path, cancellationToken, postData);
        }

        private async Task<StringResponse> PutAsync(string path, string data, CancellationToken cancellationToken)
        {
            return await DoRequestAsync(HttpMethod.PUT, path, data, cancellationToken);
        }

        private async Task<StringResponse> GetAsync(string path, string data, CancellationToken cancellationToken)
        {
            return await DoRequestAsync(HttpMethod.GET, path, data, cancellationToken);
        }

        private async Task<StringResponse> PostAsync(string path, string data, CancellationToken cancellationToken)
        {
            return await DoRequestAsync(HttpMethod.POST, path, data, cancellationToken);
        }
    }
}
