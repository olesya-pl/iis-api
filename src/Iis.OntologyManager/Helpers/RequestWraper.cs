using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Iis.OntologyManager.Helpers
{
    public class RequestWraper
    {
        private readonly string _apiAddress;

        public RequestWraper(string apiAddress)
        {
            _apiAddress = apiAddress;
        }

        public Task<bool> DeleteEntityAsync(Guid entityId) 
        {
            var requestUri = GetDeleteEntityRequestUri(entityId);

            return SendDeleteRequestAsync(requestUri);
        }

        private async Task<bool> SendDeleteRequestAsync(Uri requestUri)
        {
            using (var httpClient = GetClient(_apiAddress))
            {
                var response = await httpClient.DeleteAsync(requestUri).ConfigureAwait(false);

                response.EnsureSuccessStatusCode();

                return response.IsSuccessStatusCode;
            }
        }

        private static HttpClient GetClient(string apiAddress)
        {
            return new HttpClient
            {
                BaseAddress = new Uri(apiAddress)
            };
        }

        private static Uri GetDeleteEntityRequestUri(Guid entityId)
        {
            return new Uri($"api/entity/{entityId:N}", UriKind.Relative);
        }
    }
}
