using System;
using System.Text;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Iis.OntologyManager.Configurations;

namespace Iis.OntologyManager.Helpers
{
    public class RequestWraper
    {
        private readonly string _apiAddress;
        private readonly UserCredentials _userCredentials;

        public RequestWraper(string apiAddress, UserCredentials userCredentials)
        {
            _apiAddress = apiAddress;
            _userCredentials = userCredentials;
        }

        public Task<bool> DeleteEntityAsync(Guid entityId) 
        {
            var requestUri = GetDeleteEntityRequestUri(entityId);

            return SendDeleteRequestAsync(requestUri);
        }

        private async Task<bool> SendDeleteRequestAsync(Uri requestUri)
        {
            using (var httpClient = GetClient(_apiAddress, _userCredentials))
            {
                var response = await httpClient.DeleteAsync(requestUri).ConfigureAwait(false);

                try
                {
                    response.EnsureSuccessStatusCode();
                    return response.IsSuccessStatusCode;
                }
                catch (Exception ex)
                {
                    return response.IsSuccessStatusCode;
                }

            }
        }

        private static HttpClient GetClient(string apiAddress, UserCredentials userCredentials)
        {
            var byteArray = Encoding.ASCII.GetBytes($"{userCredentials.UserName}:{userCredentials.Password}");

            var client = new HttpClient
            {
                BaseAddress = new Uri(apiAddress)
            };

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

            return client;
        }

        private static Uri GetDeleteEntityRequestUri(Guid entityId)
        {
            return new Uri($"api/entity/{entityId:N}", UriKind.Relative);
        }
    }
}
