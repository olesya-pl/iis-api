using Elasticsearch.Net;

using Iis.Interfaces.Elastic;
using Iis.Interfaces.Ontology.Schema;
using Iis.Utility;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Iis.Elastic.SearchResult;
using Iis.Elastic.ElasticMappingProperties;
using Iis.Elastic.SearchQueryExtensions;

namespace Iis.Elastic
{
    internal class ElasticManager: IElasticManager
    {
        private const string EscapeSymbolsPattern = "^\"~:(){}[]\\/!";
        private const string RemoveSymbolsPattern = "№";
        public const string NullValue = "NULL";
        public const string AggregateSuffix = "Aggregate";
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

            var connectionPool = new StaticConnectionPool(new[] { new Uri(_configuration.Uri) });

            var config = new ConnectionConfiguration(connectionPool);

            _lowLevelClient = new ElasticLowLevelClient(config);
            _resultExtractor = resultExtractor;
            _logger = logger;
            _responseLogUtils = responseLogUtils;
        }

        public Task<bool> PutDocumentAsync(string indexName, string documentId, string jsonDocument, CancellationToken cancellationToken = default)
        {
            return PutDocumentAsync(indexName, documentId, jsonDocument, false, cancellationToken);
        }

        public async Task<bool> PutDocumentAsync(string indexName, string documentId, string jsonDocument, bool waitForIndexing, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(indexName) || string.IsNullOrWhiteSpace(documentId) || string.IsNullOrWhiteSpace(jsonDocument)) 
                return false;

            PostData postData = jsonDocument;

            var response = await _lowLevelClient.IndexAsync<StringResponse>(GetRealIndexName(indexName), documentId, postData, new IndexRequestParameters 
            {
                Refresh = waitForIndexing ? Refresh.WaitFor : Refresh.False
            });

            return response.Success;
        }

        public async Task<List<ElasticBulkResponse>> PutDocumentsAsync(string indexName, string documents, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(indexName))
                return null;

            var indexUrl = $"{GetRealIndexName(indexName)}/_bulk";
            var response = await PostAsync(indexUrl, documents, ct);

            return ParseBulkBodyResponse(response.Body);
        }

        private List<ElasticBulkResponse> ParseBulkBodyResponse(string body)
        {

            var jBody = JObject.Parse(body);
            var statusItems = jBody["items"];
            var result = new List<ElasticBulkResponse>();
            foreach (JObject item in statusItems)
            {
                if (IsErrorStatusCode(item["index"]["status"].Value<int>()))
                {
                    result.Add(new ElasticBulkResponse
                    {
                        Id = item["index"]["_id"].Value<string>(),
                        IsSuccess = false,
                        ErrorReason = item["index"]["error"]["reason"].Value<string>(),
                        ErrorType = item["index"]["error"]["type"].Value<string>(),
                    });
                }
                else
                {
                    result.Add(new ElasticBulkResponse
                    {
                        Id = item["index"]["_id"].Value<string>(),
                        IsSuccess = true,
                        SuccessOperation = item["index"]["result"].Value<string>()
                    });
                }
            }

            return result;
        }

        private ElasticResponse ParseResponse(StringResponse response) 
        {
            var result = new ElasticResponse
            {
                IsSuccess = response.Success
            };

            if (!result.IsSuccess) 
            {
                var jBody = JObject.Parse(response.Body);
                result.ErrorType = jBody["error"]["type"].Value<string>();
                result.ErrorReason = jBody["error"]["reason"].Value<string>();
            }

            return result;
        }

        private bool IsErrorStatusCode(int statusCode)
        {
            return statusCode / 100 == 4;
        }

        public async Task<bool> DeleteDocumentAsync(string indexName, string documentId)
        {
            var searchResponse = await _lowLevelClient.DeleteAsync<StringResponse>(GetRealIndexName(indexName), documentId);

            return searchResponse.Success;
        }

        public Task<IElasticSearchResult> SearchAsync(IIisElasticSearchParams searchParams, CancellationToken cancellationToken = default)
        {
            var jsonString = GetSearchJson(searchParams);

            return SearchAsync(jsonString, searchParams.BaseIndexNames, cancellationToken);
        }

        public Task<int> CountAsync(IIisElasticSearchParams searchParams, CancellationToken cancellationToken = default)
        {
            var jsonString = GetCountJson(searchParams);

            return CountAsync(jsonString, searchParams.BaseIndexNames, cancellationToken);
        }

        public Task<IElasticSearchResult> SearchAsync(IMultiElasticSearchParams searchParams, CancellationToken cancellationToken = default)
        {
            var jsonString = GetSearchJson(searchParams);

            return SearchAsync(jsonString, searchParams.BaseIndexNames, cancellationToken);
        }

        public Task<int> CountAsync(IMultiElasticSearchParams searchParams, CancellationToken cancellationToken = default)
        {
            var jsonString = GetCountJson(searchParams);

            return CountAsync(jsonString, searchParams.BaseIndexNames, cancellationToken);
        }

        public async Task<IElasticSearchResult> SearchAsync(string queryData, IEnumerable<string> baseIndexNameList, CancellationToken cancellationToken = default)
        {
            var path = !baseIndexNameList.Any() ? "_search" : $"{GetRealIndexNames(baseIndexNameList)}/_search";

            var response = await GetAsync(path, queryData, cancellationToken);

            return _resultExtractor.GetFromResponse(response);
        }

        public async Task<int> CountAsync(string queryData, IEnumerable<string> baseIndexNameList, CancellationToken cancellationToken = default)
        {
            var path = !baseIndexNameList.Any() ? "_count" : $"{GetRealIndexNames(baseIndexNameList)}/_count";

            var response = await GetAsync(path, queryData, cancellationToken);

            return JObject.Parse(response.Body)["count"].Value<int>();
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

        public async Task<IElasticSearchResult> GetDocumentByIdAsync(IReadOnlyCollection<string> indexNames, string documentId, CancellationToken token = default)
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
            mappingConfiguration.Properties.Add(KeywordProperty.Create($"NodeTypeTitle{AggregateSuffix}", false));
            var indexUrl = GetRealIndexName(attributesList.EntityTypeName);
            var jObject = mappingConfiguration.ToJObject();
            ApplyRussianAnalyzerAsync(jObject);
            ApplyIndexMappingSettings(jObject);
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

        private void ApplyIndexMappingSettings(JObject request)
        {
            var mappingSettigns = @"{
                    'total_fields': {
                        'limit': 4000
                    }
                }";

            var settigns = request["settings"];

            settigns["mapping"] = JObject.Parse(mappingSettigns);
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
                            queryVector = imageVector
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

        public async Task<ElasticResponse> AddMappingPropertyToIndexAsync(string indexName, JObject mappingConfiguration, CancellationToken ct = default) 
        {
            if (!await IndexExistsAsync(indexName, ct))
                throw new ArgumentException($"{indexName} does not exist", nameof(indexName));

            var response = await DoRequestAsync(HttpMethod.PUT, $"{GetRealIndexName(indexName)}/_mapping", mappingConfiguration.ToString(), ct);

            return ParseResponse(response);
        }

        private void ApplyMappingConfiguration(JObject request, JObject mappingConfiguration)
        {
            if (mappingConfiguration == null)
            {
                return;
            }
            request.Merge(mappingConfiguration);
        }

        private string GetSearchJson(IMultiElasticSearchParams searchParams)
        {
            var json = new JObject();
            json["_source"] = new JArray(searchParams.ResultFields);
            json["from"] = searchParams.From;
            json["size"] = searchParams.Size;
            json["query"] = new JObject();
            json["query"]["bool"] = new JObject();

            PrepareHighlights(json);
            PrepareAggregations(json, searchParams.SearchParams.SelectMany(p => p.Fields).Where(p => p.IsAggregated).ToList());

            var shouldSections = new JArray();
            foreach (var searchItem in searchParams.SearchParams)
            {
                if (SearchQueryExtension.IsExactQuery(searchItem.Query))
                {
                    var shouldSection = CreateExactShouldSection(searchItem.Query, searchParams.IsLenient);
                    shouldSections.Add(shouldSection);
                }
                else if (searchItem.Fields?.Any() == true)
                {
                    var shouldSection = CreateMultiFieldShouldSection(searchItem.Query, searchItem.Fields, searchParams.IsLenient);
                    shouldSections.Merge(shouldSection);
                }
                else
                {
                    var shouldSection = CreateFallbackShouldSection(searchItem.Query, searchParams.IsLenient);
                    shouldSections.Add(shouldSection);
                }
            }

            json["query"]["bool"]["should"] = shouldSections;
            return json.ToString();
        }

        private void PrepareAggregations(JObject json, List<IIisElasticField> fields)
        {
            const int MaxBucketsCount = 100;
            if (!fields.Any())
            {
                return;
            }
            var aggs = new JObject();
            json["aggs"] = aggs;
            foreach (var field in fields)
            {
                var fieldObj = new JObject();
                fieldObj["field"] = $"{field.Name}{AggregateSuffix}";
                fieldObj["size"] = MaxBucketsCount;
                var terms = new JObject();
                terms["terms"] = fieldObj;
                aggs[field.Name] = terms;
            }
        }

        private string GetCountJson(IMultiElasticSearchParams searchParams)
        {
            var json = new JObject();
            json["query"] = new JObject();
            json["query"]["bool"] = new JObject();

            var shouldSections = new JArray();
            foreach (var searchItem in searchParams.SearchParams)
            {
                if (SearchQueryExtension.IsExactQuery(searchItem.Query))
                {
                    var shouldSection = CreateExactShouldSection(searchItem.Query, searchParams.IsLenient);
                    shouldSections.Add(shouldSection);
                }
                else if (searchItem.Fields?.Any() == true)
                {
                    var shouldSection = CreateMultiFieldShouldSection(searchItem.Query, searchItem.Fields, searchParams.IsLenient);
                    shouldSections.Merge(shouldSection);
                }
                else
                {
                    var shouldSection = CreateFallbackShouldSection(searchItem.Query, searchParams.IsLenient);
                    shouldSections.Add(shouldSection);
                }
            }

            json["query"]["bool"]["should"] = shouldSections;
            return json.ToString();
        }

        private string GetSearchJson(IIisElasticSearchParams searchParams)
        {
            var json = new JObject();
            json["_source"] = new JArray(searchParams.ResultFields);
            json["from"] = searchParams.From;
            json["size"] = searchParams.Size;
            json["query"] = new JObject();

            PrepareHighlights(json);
            if(!string.IsNullOrEmpty(searchParams.SortColumn) && !string.IsNullOrEmpty(searchParams.SortOrder))
            {
                json["sort"] = new JArray()
                {
                    CreateSortSection(searchParams.SortColumn, searchParams.SortOrder)
                };
            }

            if (SearchQueryExtension.IsExactQuery(searchParams.Query) && !searchParams.SearchFields.Any())
            {
                PopulateExactQuery(searchParams, json);
            }
            else if (searchParams.SearchFields.Any())
            {
                PopulateFieldsIntoQuery(searchParams, json);
            }
            else
            {
                PrepareFallbackQuery(searchParams, json);
            }

            return json.ToString();
        }

        private string GetCountJson(IIisElasticSearchParams searchParams)
        {
            var json = new JObject();
            json["query"] = new JObject();

            if (SearchQueryExtension.IsExactQuery(searchParams.Query) && !searchParams.SearchFields.Any())
            {
                PopulateExactQuery(searchParams, json);
            }
            else if (searchParams.SearchFields.Any())
            {
                PopulateFieldsIntoQuery(searchParams, json);
            }
            else
            {
                PrepareFallbackQuery(searchParams, json);
            }

            return json.ToString();
        }

        private JObject CreateSortSection(string sortColumName, string sortOder) 
        {
            var result = new JObject();
            result.Add(sortColumName, new JObject() { new JProperty("order", sortOder) });
            return result;
        }        

        private void PopulateExactQuery(IIisElasticSearchParams searchParams, JObject json)
        {
            var queryString = new JObject();
            queryString["query"] = searchParams.Query;
            queryString["lenient"] = searchParams.IsLenient;
            json["query"]["query_string"] = queryString;
        }

        private JObject CreateExactShouldSection(string query, bool isLenient) 
        {
            var result = new JObject();

            var queryString = new JObject();
            queryString["query"] = query;
            queryString["lenient"] = isLenient;
            result["query_string"] = queryString;

            return result;
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

        private JArray CreateMultiFieldShouldSection(string query, List<IIisElasticField> searchFields, bool isLenient) 
        {
            var shouldSections = new JArray();

            foreach (var searchFieldGroup in searchFields.GroupBy(p => new { p.Fuzziness, p.Boost }))
            {
                var querySection = new JObject();
                var queryString = new JObject();
                queryString["query"] = ApplyFuzzinessOperator(
                    EscapeElasticSpecificSymbols(RemoveSymbols(query, RemoveSymbolsPattern), 
                    EscapeSymbolsPattern));
                queryString["fuzziness"] = searchFieldGroup.Key.Fuzziness;
                queryString["boost"] = searchFieldGroup.Key.Boost;
                queryString["lenient"] = isLenient;
                queryString["fields"] = new JArray(searchFieldGroup.Select(p => p.Name));
                
                querySection["query_string"] = queryString;
                shouldSections.Add(querySection);
            }

            return shouldSections;
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

        private JObject CreateFallbackShouldSection(string query, bool isLenient) 
        {
            var shouldSection = new JObject();
            var queryString = new JObject();
            queryString["query"] = EscapeElasticSpecificSymbols(
                RemoveSymbols(query, RemoveSymbolsPattern), EscapeSymbolsPattern);
            queryString["fields"] = new JArray("*");
            queryString["lenient"] = isLenient;
            shouldSection["query_string"] = queryString;

            return queryString;
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

            return $"\"{input}\" OR {input}~";
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
