using Elasticsearch.Net;
using Iis.Domain.Elastic;
using Iis.Domain.ExtendedData;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Iis.Elastic
{
    public class IisElasticManager: IElasticManager
    {
        ElasticLowLevelClient _lowLevelClient;
        IisElasticConfiguration _configuration;
        IisElasticSerializer _serializer;
        
        public IisElasticManager(IisElasticConfiguration configuration, IisElasticSerializer serializer)
        {
            _configuration = configuration;
            _serializer = serializer;

            var connectionPool = new SniffingConnectionPool(new[] { new Uri(_configuration.Uri) });
            var config = new ConnectionConfiguration(connectionPool);
            _lowLevelClient = new ElasticLowLevelClient(config);
        }

        public async Task<bool> PutJsonAsync(string baseIndexName, string id, string json, CancellationToken cancellationToken = default)
        {
            var path = $"{GetRealIndexName(baseIndexName)}/_doc/{id}";
            PostData postData = json;
            var response = await _lowLevelClient.DoRequestAsync<StringResponse>(HttpMethod.PUT, path, cancellationToken, postData);
            return response.Success;
        }

        public async Task<bool> PutExtNodeAsync(ExtNode extNode, CancellationToken cancellationToken = default)
        {
            var json = _serializer.GetJsonByExtNode(extNode);
            return await PutJsonAsync(extNode.NodeTypeName, extNode.Id, json, cancellationToken);
        }

        public async Task<List<string>> Search(IisElasticSearchParams searchParams, CancellationToken cancellationToken = default)
        {
            var jsonString = GetSearchJson(searchParams);
            var path = searchParams.BaseIndexNames.Count == 0 ? 
                "_search" : 
                $"{GetRealIndexNames(searchParams.BaseIndexNames)}/_search";

            var response = await GetAsync(path, jsonString, cancellationToken);
            return GetIdsFromSearchResponse(response);
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

        private string GetSearchJson(IisElasticSearchParams searchParams)
        {
            var json = new JObject();
            json["_source"] = new JArray(searchParams.ResultFields);
            json["query"] = new JObject();
            var queryString = new JObject();
            queryString["query"] = searchParams.Query;
            queryString["fields"] = new JArray(searchParams.SearchFields);
            queryString["lenient"] = searchParams.IsLenient;

            json["query"]["query_string"] = queryString;
            return json.ToString();
        }

        private string GetRealIndexName(string baseIndexName)
        {
            return $"{_configuration.IndexPreffix}{baseIndexName}".ToLower();
        }

        private string GetRealIndexNames(List<string> baseIndexNames)
        {
            return string.Join(',', baseIndexNames.Select(name => GetRealIndexName(name)));
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
