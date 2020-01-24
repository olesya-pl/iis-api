using Elasticsearch.Net;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Iis.Elastic
{
    public class IisElasticManager
    {
        ElasticLowLevelClient _lowLevelClient;
        IisElasticConfiguration _configuration;
        
        public IisElasticManager(IisElasticConfiguration configuration)
        {
            _configuration = configuration;

            var connectionPool = new SniffingConnectionPool(new[] { new Uri(_configuration.Uri) });
            var config = new ConnectionConfiguration(connectionPool);
            _lowLevelClient = new ElasticLowLevelClient(config);
        }

        public async Task<bool> InsertJsonAsync(string baseIndexName, string id, string json, CancellationToken cancellationToken)
        {
            var path = $"{GetRealIndexName(baseIndexName)}/{id}";
            PostData postData = json;
            var response = await _lowLevelClient.DoRequestAsync<StringResponse>(HttpMethod.PUT, $"test/person/{id}", cancellationToken, postData);
            return response.Success;
        }

        private string GetRealIndexName(string baseIndexName)
        {
            return $"{_configuration.IndexPreffix}{baseIndexName}";
        }

        private async Task<StringResponse> DoRequestAsync(HttpMethod httpMethod, string path, string data, CancellationToken cancellationToken)
        {
            PostData postData = data;
            return await _lowLevelClient.DoRequestAsync<StringResponse>(HttpMethod.PUT, path, cancellationToken, postData);
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
