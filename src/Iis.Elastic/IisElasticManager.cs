using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Elasticsearch.Net;
using Newtonsoft.Json.Linq;
using Iis.Domain.Elastic;
using Iis.Domain.ExtendedData;
using Iis.Interfaces.Elastic;
using Iis.Interfaces.Ontology;

namespace Iis.Elastic
{
    public class IisElasticManager: IElasticManager
    {
        private const string EscapeSymbolsPattern = "^\"~:(){}[]\\/";
        ElasticLowLevelClient _lowLevelClient;
        IisElasticConfiguration _configuration;
        IisElasticSerializer _serializer;
        public List<string> SupportedIndexes { get; private set; }
        
        public IisElasticManager(IisElasticConfiguration configuration, IisElasticSerializer serializer)
        {
            _configuration = configuration;
            _serializer = serializer;

            var connectionPool = new SniffingConnectionPool(new[] { new Uri(_configuration.Uri) });
            var config = new ConnectionConfiguration(connectionPool);
            _lowLevelClient = new ElasticLowLevelClient(config);

            SupportedIndexes = new List<string> {
                "Organization",
                "Person",
                "ObjectOfStudy",
                "Radionetwork",
                "MilitaryMachinery",
                "Unknown",
                "MilitaryBase",
                "Infrastructure",
                "Subdivision",
                "SecondarySpecialEducationalInstitution",
                "HigherEducationalInstitution",
                "EducationalInstitution"
            };
        }

        public bool IndexIsSupported(string indexName)
        {
            return SupportedIndexes.Contains(indexName);
        }

        public bool IndexesAreSupported(IEnumerable<string> indexNames)
        {
            return indexNames.All(name => IndexIsSupported(name));
        }

        public async Task<bool> PutJsonAsync(string baseIndexName, string id, string json, CancellationToken cancellationToken = default)
        {
            var path = $"{GetRealIndexName(baseIndexName)}/_doc/{id}";
            PostData postData = json;
            var response = await _lowLevelClient.DoRequestAsync<StringResponse>(HttpMethod.PUT, path, cancellationToken, postData);
            return response.Success;
        }

        public async Task<bool> PutExtNodeAsync(IExtNode extNode, CancellationToken cancellationToken = default)
        {
            if (!IndexIsSupported(extNode.NodeTypeName)) return false;
            var json = _serializer.GetJsonByExtNode(extNode);
            return await PutJsonAsync(extNode.NodeTypeName, extNode.Id, json, cancellationToken);
        }

        public async Task<List<string>> Search(IIisElasticSearchParams searchParams, CancellationToken cancellationToken = default)
        {
            var jsonString = GetSearchJson(searchParams);
            var path = searchParams.BaseIndexNames.Count == 0 ? 
                "_search" : 
                $"{GetRealIndexNames(searchParams.BaseIndexNames)}/_search";

            var response = await GetAsync(path, jsonString, cancellationToken);
            return GetIdsFromSearchResponse(response);
        }

        public async Task<bool> DeleteAllIndexes(CancellationToken cancellationToken = default)
        {
            var path = $"{_configuration.IndexPreffix}*";
            var response = await DoRequestAsync(HttpMethod.DELETE, path, string.Empty, cancellationToken);
            return response.Success;
        }


        public async Task<List<string>> GetIndexIdsAsync(string indexName)
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
                return new List<string>();
            }
            
            return GetIdsFromSearchResponse(searchResponse);
        }

        public async Task<string> GetByIdAsync(string indexName, string id, string[] fields)
        {
            var searchResponse = await _lowLevelClient.SearchAsync<StringResponse>(GetRealIndexName(indexName), PostData.Serializable(new
            {
                query = new
                {
                    match = new
                    {
                        _id = id
                    }
                },
                _source = fields
            }));

            if (!searchResponse.Success)
            {
                return string.Empty;
            }

            return searchResponse.Body;
        }

        public async Task<bool> DeleteAsync(string indexName, string id)
        {
            var searchResponse = await _lowLevelClient.DeleteAsync<StringResponse>(GetRealIndexName(indexName), id);
            return searchResponse.Success;
        }

        private List<string> GetIdsFromSearchResponse(StringResponse response)
        {
            var json = JObject.Parse(response.Body);
            var ids = new List<string>();

            var hits = json["hits"]["hits"];
            if (hits == null) return ids;

            foreach (var hit in hits)
            {
                var hitObj = JObject.Parse(hit.ToString());
                ids.Add(hit["_id"].ToString());
                
            }

            return ids;
        }

        private string GetSearchJson(IIisElasticSearchParams searchParams)
        {
            var json = new JObject();
            json["_source"] = new JArray(searchParams.ResultFields);
            json["query"] = new JObject();
            var queryString = new JObject();

            queryString["query"] = EscapeElasticSpecificSymbols(searchParams.Query, EscapeSymbolsPattern);
            queryString["fields"] = new JArray(searchParams.SearchFields);
            queryString["lenient"] = searchParams.IsLenient;

            json["query"]["query_string"] = queryString;

            return json.ToString(); ;
        }

        private string GetRealIndexName(string baseIndexName)
        {
            return $"{_configuration.IndexPreffix}{baseIndexName}".ToLower();
        }

        private string GetRealIndexNames(List<string> baseIndexNames)
        {
            return string.Join(',', baseIndexNames.Select(name => GetRealIndexName(name)));
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
