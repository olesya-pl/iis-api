using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Elasticsearch.Net;
using Newtonsoft.Json.Linq;
using Iis.Interfaces.Elastic;
using Iis.Interfaces.Ontology.Schema;
using Microsoft.Extensions.Logging;
using Iis.Utility;

namespace Iis.Elastic
{
    internal class ElasticManager: IElasticManager
    {
        private const string EscapeSymbolsPattern = "^\"~:(){}[]\\/";
        private const string RemoveSymbolsPattern = "№";
        public const string NullValue = "NULL";
        private readonly ElasticLowLevelClient _lowLevelClient;
        private readonly ElasticConfiguration _configuration;
        private readonly SearchResultExtractor _resultExtractor;
        private readonly ILogger<ElasticManager> _logger;
        private readonly ElasticLogUtils _responseLogUtils;

        public ElasticManager(ElasticConfiguration configuration,
            SearchResultExtractor resultExtractor,
            ILogger<ElasticManager> logger,
            ElasticLogUtils responseLogUtils)
        {
            _configuration = configuration;

            var connectionPool = new SniffingConnectionPool(new[] { new Uri(_configuration.Uri) });

            var config = new ConnectionConfiguration(connectionPool);

            _lowLevelClient = new ElasticLowLevelClient(config);
            _resultExtractor = resultExtractor;
            _logger = logger;
            _responseLogUtils = responseLogUtils;
        }

        public async Task<bool> PutDocumentAsync(string indexName, string documentId, string jsonDocument, CancellationToken cancellationToken = default)
        {
            if(string.IsNullOrWhiteSpace(indexName) || string.IsNullOrWhiteSpace(documentId) || string.IsNullOrWhiteSpace(jsonDocument)) return false;

            var indexUrl = $"{GetRealIndexName(indexName)}/_doc/{documentId}";

            PostData postData = jsonDocument;

            var response = await _lowLevelClient.DoRequestAsync<StringResponse>(HttpMethod.PUT, indexUrl, cancellationToken, postData);

            return response.Success;
        }

        public async Task<bool> PutsDocumentsAsync(string indexName, string materialDocuments, CancellationToken token)
        {
            if (string.IsNullOrWhiteSpace(indexName)) return false;
            var indexUrl = $"{GetRealIndexName(indexName)}/_bulk";
            var response = await PostAsync(indexUrl, materialDocuments, token);
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
            return _resultExtractor.GetFromResponse(response);
        }

        public async Task<IElasticSearchResult> SearchMoreLikeThisAsync(IIisElasticSearchParams searchParams, CancellationToken cancellationToken = default)
        {
            var json = JObject.Parse(
                @"{
                    '_source': ['_id'],
                    'from':0,
                    'size':10,
                    'query':{
                        'bool':{
                            'must':[
                                {'term': {'ParentId':'NULL'}},
                                {'more_like_this': {
                                        'fields': [ 'Content' ],
                                        'like' : [ { '_id': '' } ],
                                        'min_term_freq' : 1
                                    }
                                }
                            ]
                        }
                    }
                }"
            );

            json["_source"] = new JArray(searchParams.ResultFields);

            json["from"] = searchParams.From;

            json["size"] = searchParams.Size;

            json["query"]["bool"]["must"][1]["more_like_this"]["like"][0]["_id"] = searchParams.Query;

            var query = json.ToString(Newtonsoft.Json.Formatting.None);

            var path = searchParams.BaseIndexNames.Count == 0 ?
                "_search" :
                $"{GetRealIndexNames(searchParams.BaseIndexNames)}/_search";

            var response = await GetAsync(path, query, cancellationToken);

            return _resultExtractor.GetFromResponse(response);
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

            return _resultExtractor.GetFromResponse(searchResponse);
        }

        public async Task<IElasticSearchResult> GetDocumentByIdAsync(IReadOnlyCollection<string> indexNames,
            string documentId,
            CancellationToken token = default)
        {
            var searchResponse = await _lowLevelClient.SearchAsync<StringResponse>(
                GetRealIndexNames(indexNames),
                PostData.Serializable(new
                {
                    query = new
                    {
                        match = new
                        {
                            _id = documentId
                        }
                    },
                    _source = "*"
                }), null, token);

            if (!searchResponse.Success)
            {
                return new ElasticSearchResult();
            }

            return _resultExtractor.GetFromResponse(searchResponse);
        }

        public async Task CreateIndexesAsync(IEnumerable<string> indexNames, JObject mappingConfiguration = null, CancellationToken token = default)
        {
            foreach (string indexName in indexNames)
            {
                if (!await IndexExistsAsync(indexName, token))
                {
                    await CreateIndexAsync(indexName, mappingConfiguration, token);
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

        public async Task<bool> CreateMapping(IAttributeInfoList attributesList, CancellationToken cancellationToken = default)
        {
            var mappingConfiguration = new ElasticMappingConfiguration(attributesList);
            var indexUrl = GetRealIndexName(attributesList.EntityTypeName);
            var jObject = mappingConfiguration.ToJObject();
            ApplyRussianAnalyzerAsync(jObject);
            var response = await DoRequestAsync(HttpMethod.PUT, indexUrl, jObject.ToString(), cancellationToken);
            return response.Success;
        }

        public void ApplyRussianAnalyzerAsync(JObject createRequest)
        {
            var analyzerSettings = @"{
            ""analysis"": {
                ""filter"": {
                    ""russian_stop"": {
                        ""type"": ""stop"",
                        ""stopwords"": ""_russian_""
                    },
                    ""russian_stemmer"": {
                        ""type"": ""stemmer"",
                        ""language"": ""russian""
                    }
                },
            ""analyzer"": {
                ""default"": {
                    ""tokenizer"": ""standard"",
                    ""filter"": [
                        ""lowercase"",
                        ""russian_stop"",
                        ""russian_stemmer""
                ]}}}}";
            createRequest["settings"] = JObject.Parse(analyzerSettings);
        }

        public async Task<IElasticSearchResult> SearchByImageVector(decimal[] imageVector, IIisElasticSearchParams searchParams, CancellationToken token)
        {
            var searchResponse = await _lowLevelClient.SearchAsync<StringResponse>(index:GetRealIndexNames(searchParams.BaseIndexNames), PostData.Serializable(new
            {
                from = searchParams.From,
                size = searchParams.Size,
                min_score = 0.1,
                query = new {
                    script_score = new {
                    query = new {
                        match_all = new { }
                    },
                    script = new {
                        source = "1 / (l2norm(params.queryVector, doc['ImageVector']) + 1)",
                        @params = new {
                            queryVector =  imageVector
                        }
                    }
                }
             }
            }),
            ctx: token);
            return _resultExtractor.GetFromResponse(searchResponse);
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

        private async Task<bool> CreateIndexAsync(string indexName, JObject mappingConfiguration = null, CancellationToken token = default)
        {
            var request = new JObject();
            ApplyRussianAnalyzerAsync(request);
            ApplyMappingConfiguration(request, mappingConfiguration);
            var response =
                await DoRequestAsync(HttpMethod.PUT, GetRealIndexName(indexName), request.ToString(), token);
            return response.Success;
        }

        private void ApplyMappingConfiguration(JObject request, JObject mappingConfiguration)
        {
            if (mappingConfiguration == null)
            {
                return;
            }
            request.Merge(mappingConfiguration);
        }

        private string GetSearchJson(IIisElasticSearchParams searchParams)
        {
            var json = new JObject();
            json["_source"] = new JArray(searchParams.ResultFields);
            json["from"] = searchParams.From;
            json["size"] = searchParams.Size;
            json["query"] = new JObject();

            PrepareHighlights(json);

            if (IsExactQuery(searchParams.Query))
            {
                PopulateExactQuery(searchParams, json);
            }
            else if (searchParams.SearchFields?.Any() == true)
            {
                PopulateFieldsIntoQuery(searchParams, json);
            }
            else
            {
                PrepareFallbackQuery(searchParams, json);
            }

            return json.ToString();
        }

        private bool IsExactQuery(string query)
        {
            return query.Contains(":")
                || query.Contains(" AND ")
                || query.Contains(" OR ");
        }

        private void PopulateExactQuery(IIisElasticSearchParams searchParams, JObject json)
        {
            var queryString = new JObject();
            queryString["query"] = searchParams.Query;
            queryString["lenient"] = searchParams.IsLenient;
            json["query"]["query_string"] = queryString;
        }

        private void PopulateFieldsIntoQuery(IIisElasticSearchParams searchParams, JObject json)
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
            json["highlight"]["fields"]["*"] = JObject.Parse("{\"type\" : \"unified\"}");
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
            if (IsWildCard(input))
            {
                return input;
            }

            return $"\"{input}\" OR \"{input}\"~";
        }

        private static bool IsWildCard(string input)
        {
            return input.Contains('*');
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
            using (DurationMeter.Measure($"Elastic request {httpMethod} {path}", _logger))
            {
                PostData postData = data;
                var response = await _lowLevelClient.DoRequestAsync<StringResponse>(httpMethod, path, cancellationToken, postData);
                var log = _responseLogUtils.PrepareLog(response);
                _logger.Log(log.LogLevel, log.Message);
                return response;
            }
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
