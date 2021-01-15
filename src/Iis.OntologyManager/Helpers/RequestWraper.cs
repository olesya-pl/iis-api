using System;
using System.Text;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Serilog;
using Iis.OntologyManager.Configurations;
using Iis.OntologyManager.DTO;

namespace Iis.OntologyManager.Helpers
{
    public class RequestWraper
    {
        private readonly UserCredentials _userCredentials;
        private readonly Uri _baseApiApiAddress;
        private readonly ILogger _logger;

        public RequestWraper(string apiAddress, UserCredentials userCredentials, ILogger logger)
        {
            _baseApiApiAddress = new Uri(apiAddress);
            _userCredentials = userCredentials;
            _logger = logger;
        }

        public Task<RequestResult> DeleteEntityAsync(Guid entityId) 
        {
            var requestUri = GetDeleteEntityRequestUri(entityId);

            return SendDeleteRequestAsync(requestUri);
        }

        private async Task<RequestResult> SendDeleteRequestAsync(Uri requestUri)
        {
            using var httpClient = GetClient(_baseApiApiAddress, _userCredentials);
            var response = await httpClient.DeleteAsync(requestUri).ConfigureAwait(false);
            try
            {
                response.EnsureSuccessStatusCode();

                return RequestResult.Success("Сутність видалено", response.RequestMessage.RequestUri);
            }
            catch(Exception exception)
            {
                _logger.Error($"Uri:{response.RequestMessage.RequestUri} Exception:{exception}");

                return RequestResult.Fail($"Code={response.StatusCode}:{response.ReasonPhrase}", response.RequestMessage.RequestUri);
            }
        }

        private static HttpClient GetClient(Uri apiAddress, UserCredentials userCredentials)
        {
            var byteArray = Encoding.ASCII.GetBytes($"{userCredentials.UserName}:{userCredentials.Password}");

            var client = new HttpClient
            {
                BaseAddress = apiAddress
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
