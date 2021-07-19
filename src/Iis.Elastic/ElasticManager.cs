﻿using Elasticsearch.Net;

using Iis.Interfaces.Elastic;
using Iis.Interfaces.Ontology.Schema;
using Iis.Utility;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Iis.Elastic.SearchResult;
using Iis.Elastic.ElasticMappingProperties;
using Iis.Elastic.SearchQueryExtensions;
using Newtonsoft.Json;
using System.Text;
using System.Text.RegularExpressions;

namespace Iis.Elastic
{
    public class ElasticManager : IElasticManager
    {
        private ElasticLowLevelClient _lowLevelClient;
        private readonly ElasticConfiguration _configuration;
        private readonly SearchResultExtractor _resultExtractor;
        private readonly ILogger<ElasticManager> _logger;
        private readonly ElasticLogUtils _responseLogUtils;
        public const string NullValue = "NULL";
        public static readonly HashSet<char> RemoveSymbolsPattern = new HashSet<char> { '№' };
        public static readonly HashSet<char> EscapeSymbolsPattern = new HashSet<char> { '^',   ':', '{', '}', '[', ']', '/', '!' };

        public ElasticManager(ElasticConfiguration configuration,
            SearchResultExtractor resultExtractor,
            ILogger<ElasticManager> logger,
            ElasticLogUtils responseLogUtils)
        {
            _configuration = configuration;
            CreateLowlevelClient(_configuration.DefaultLogin, _configuration.DefaultPassword);

            _resultExtractor = resultExtractor;
            _logger = logger;
            _responseLogUtils = responseLogUtils;
        }

        private void CreateLowlevelClient(string login, string password)
        {
            var connectionPool = new StaticConnectionPool(new[] { new Uri(_configuration.Uri) });
            var config = new ConnectionConfiguration(connectionPool)
                .BasicAuthentication(login, password)
                .DisablePing();
            _lowLevelClient = new ElasticLowLevelClient(config);
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
            var result = new List<ElasticBulkResponse>(statusItems.Count());
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

        public async Task<bool> DeleteDocumentAsync(string indexName, string documentId, CancellationToken ct = default)
        {
            var searchResponse = await _lowLevelClient.DeleteAsync<StringResponse>(GetRealIndexName(indexName), documentId, ctx: ct);

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

        public async Task<IElasticSearchResult> SearchAsync(string queryData, IEnumerable<string> baseIndexNameList, CancellationToken cancellationToken = default)
        {
            var path = !baseIndexNameList.Any() ? "_search" : $"{GetRealIndexNames(baseIndexNameList)}/_search";

            var response = await GetAsync(path, queryData, null, cancellationToken);

            return _resultExtractor.GetFromResponse(response);
        }

        public async Task<IElasticSearchResult> BeginSearchByScrollAsync(
            string queryData,
            TimeSpan scrollLifetime,
            IEnumerable<string> baseIndexNameList,
            CancellationToken cancellationToken = default)
        {
            var path = !baseIndexNameList.Any() ? "_search" : $"{GetRealIndexNames(baseIndexNameList)}/_search";
            var queryString = new Dictionary<string, object>()
            {
                {"scroll",  $"{(int)scrollLifetime.TotalSeconds}s"}
            };
            var response = await GetAsync(path, queryData, queryString, cancellationToken);

            return _resultExtractor.GetFromResponse(response);
        }

        public async Task<IElasticSearchResult> SearchByScrollAsync(string scrollId, TimeSpan scrollDuration)
        {
            var path = "_search/scroll";
            var postData = $@"{{
                ""scroll"" : ""{ scrollDuration.TotalSeconds }s"",                                                                 
                ""scroll_id"" : ""{scrollId}""
            }}";
            var response = await PostAsync(path, postData, CancellationToken.None);
            return _resultExtractor.GetFromResponse(response);
        }

        public async Task<int> CountAsync(string queryData, IEnumerable<string> baseIndexNameList, CancellationToken cancellationToken = default)
        {
            var path = !baseIndexNameList.Any() ? "_count" : $"{GetRealIndexNames(baseIndexNameList)}/_count";

            var response = await GetAsync(path, queryData, null, cancellationToken);

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

            var response = await DoRequestAsync(HttpMethod.DELETE, path, string.Empty, null, cancellationToken);

            return response.Success;
        }

        public async Task<bool> DeleteIndexAsync(string indexName, CancellationToken cancellationToken = default)
        {
            var indexUrl = $"{GetRealIndexName(indexName)}";

            var response = await DoRequestAsync(HttpMethod.DELETE, indexUrl, string.Empty, null, cancellationToken);

            return response.Success;
        }

        public async Task<bool> CreateMapping(IAttributeInfoList attributesList, CancellationToken cancellationToken = default)
        {
            var mappingConfiguration = new ElasticMappingConfiguration(attributesList);
            mappingConfiguration.Properties.Add(KeywordProperty.Create($"{ElasticConfigConstants.NodeTypeTitleField}{SearchQueryExtension.AggregateSuffix}", false));
            mappingConfiguration.Properties.Add(TextProperty.Create(ElasticConfigConstants.NodeTypeTitleField, true));
            mappingConfiguration.Properties.Add(AliasProperty.Create(ElasticConfigConstants.NodeTypeTitleAlias, ElasticConfigConstants.NodeTypeTitleAggregateField));
            mappingConfiguration.Properties.Add(DateProperty.Create(ElasticConfigConstants.CreatedAtField, ElasticConfiguration.DefaultDateFormats));
            mappingConfiguration.Properties.Add(DateProperty.Create(ElasticConfigConstants.UpdatedAtField, ElasticConfiguration.DefaultDateFormats));

            var indexUrl = GetRealIndexName(attributesList.EntityTypeName);
            var jObject = mappingConfiguration.ToJObject();
            ApplyRussianAnalyzerAsync(jObject);
            ApplyIndexMappingSettings(jObject);
            var response = await DoRequestAsync(HttpMethod.PUT, indexUrl, jObject.ToString(), null, cancellationToken);
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
            var mappingValue = new JObject(
                new JProperty("total_fields", new JObject(
                    new JProperty("limit", _configuration.TotalFieldsLimit)
                ))
            );

            var settigns = request["settings"];

            settigns["mapping"] = mappingValue;
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
            ctx: token);

            return searchResponse.Success || searchResponse.HttpStatusCode != 404;
        }

        private async Task<bool> CreateIndexAsync(string indexName, JObject mappingConfiguration = null, CancellationToken token = default)
        {
            var request = new JObject();
            ApplyRussianAnalyzerAsync(request);
            ApplyMappingConfiguration(request, mappingConfiguration);
            var response =
                await DoRequestAsync(HttpMethod.PUT, GetRealIndexName(indexName), request.ToString(), null, token);
            return response.Success;
        }

        public async Task<ElasticResponse> AddMappingPropertyToIndexAsync(string indexName, JObject mappingConfiguration, CancellationToken ct = default)
        {
            if (!await IndexExistsAsync(indexName, ct))
                throw new ArgumentException($"{indexName} does not exist", nameof(indexName));

            var response = await DoRequestAsync(HttpMethod.PUT, $"{GetRealIndexName(indexName)}/_mapping", mappingConfiguration.ToString(), null, ct);

            return ParseResponse(response);
        }

        public async Task<bool> CreateSecurityMappingAsync(
            List<(
                IReadOnlyCollection<string> indexNames, 
                string accessLevelFieldName)> parameters,
            CancellationToken cancellationToken)
        {
            var settings = new StringBuilder("{\"indices\": [");
            for (int i = 0; i < parameters.Count; i++)
            {
                var param = parameters[i];
                if (i > 0) settings.Append(",");

                var stringifiedIndexes = string.Join(',', param.indexNames.Select(p => $"\"{GetRealIndexName(p)}\""));
                settings.AppendLine();
                var indexSection = $@"
{{
    ""names"": [{stringifiedIndexes}],
    ""privileges"": [""read""],
    ""query"": {{
        ""template"": {{
            ""source"": ""{{\""bool\"":{{\""filter\"":[{{\""range\"":{{\""{param.accessLevelFieldName}\"":{{\""lte\"":\""{{{{_user.metadata.accessLevel}}}}\""}}}}}}]}}}}""
        }}
    }}
}}";
                settings.AppendLine(indexSection);
            }
            settings.AppendLine("]}");

            var response = await PutAsync($"_xpack/security/role/{ElasticConstants.SecurityPolicyName}", settings.ToString(), cancellationToken);
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
            if (!string.IsNullOrEmpty(searchParams.SortColumn) && !string.IsNullOrEmpty(searchParams.SortOrder))
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
                    searchParams.Query
                        .RemoveSymbols(RemoveSymbolsPattern)
                        .EscapeSymbols(EscapeSymbolsPattern));
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
            queryString["query"] = searchParams.Query
                    .RemoveSymbols(RemoveSymbolsPattern)
                    .EscapeSymbols(EscapeSymbolsPattern);
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

        public static string ApplyFuzzinessOperator(string input)
        {
            if (IsWildCard(input))
            {
                return input;
            }

            if(IsDoubleQuoted(input))
            {
                return $"{input} OR {input}~";
            }

            return $"\"{input}\" OR {input}~";
        }

        private static bool IsWildCard(string input)
        {
            return input.Contains('*');
        }

        private static bool IsDoubleQuoted(string input)
        {
            return input.StartsWith('\"') && input.EndsWith('\"');
        }

        private async Task<StringResponse> DoRequestAsync(HttpMethod httpMethod, string path, string data, IRequestParameters requestParameters, CancellationToken cancellationToken)
        {
            using (DurationMeter.Measure($"Elastic request {httpMethod} {path}", _logger))
            {
                PostData postData = data;
                var response = await _lowLevelClient.DoRequestAsync<StringResponse>(httpMethod, path, cancellationToken, postData, requestParameters);
                var log = _responseLogUtils.PrepareLog(response);
                _logger.Log(log.LogLevel, log.Message);
                return response;
            }
        }

        public async Task<bool> PutExactPayloadAsync(string path, string data, CancellationToken cancellationToken)
        {
            var result = await PutAsync(path, data, cancellationToken);
            return result.Success;
        }

        public async Task<bool> DeleteExactPayloadAsync(string path, CancellationToken cancellationToken)
        {
            var result = await DeleteAsync(path, cancellationToken);
            return result.Success;
        }

        public async Task<T> GetExactPayloadAsyncDictionary<T>(string path, CancellationToken cancellationToken)
        {
            var result = await GetAsync(path, null, null, cancellationToken);
            var jsonSerializerSettings = new JsonSerializerSettings
            {
                MissingMemberHandling = MissingMemberHandling.Ignore
            };
            return JsonConvert.DeserializeObject<T>(result.Body, jsonSerializerSettings);
        }

        public IElasticManager WithUserId(Guid userId)
        {
            CreateLowlevelClient(userId.ToString("N"), ElasticConstants.DefaultPassword);
            return this;
        }

        private async Task<StringResponse> PutAsync(string path, string data, CancellationToken cancellationToken)
        {
            return await DoRequestAsync(HttpMethod.PUT, path, data, null, cancellationToken);
        }

        private async Task<StringResponse> DeleteAsync(string path, CancellationToken cancellationToken)
        {
            return await DoRequestAsync(HttpMethod.DELETE, path, null, null, cancellationToken);
        }

        private async Task<StringResponse> GetAsync(string path, string data, Dictionary<string, object> queryString, CancellationToken cancellationToken)
        {
            IRequestParameters parameters = null;
            if (queryString != null) 
            {
                parameters = new GetRequestParameters()
                {
                    QueryString = queryString
                };
            }
            return await DoRequestAsync(HttpMethod.GET, path, data, parameters, cancellationToken);
        }

        private async Task<StringResponse> PostAsync(string path, string data, CancellationToken cancellationToken)
        {
            return await DoRequestAsync(HttpMethod.POST, path, data, null, cancellationToken);
        }        
    }
}
