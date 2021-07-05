using Iis.Services.Contracts.Interfaces;
using Iis.Services.Contracts.Matrix;
using Iis.Services.MatrixServices;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Iis.Services.MatrixServices
{
    public class MatrixService : IMatrixService
    {
        private MatrixServiceConfiguration _configuration;
        private string _accessToken;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly Uri _baseUri;
        private bool LoggedIn => _accessToken != null;
        private readonly ILogger<MatrixService> _logger;
        public bool AutoCreateUsers => _configuration.CreateUsers;

        public MatrixService(
            MatrixServiceConfiguration configuration, 
            ILoggerFactory loggerFactory, 
            IHttpClientFactory httpClientFactory)
        {
            _configuration = configuration;
            _logger = loggerFactory.CreateLogger<MatrixService>();
            _baseUri = new Uri(new Uri(_configuration.Server), "_matrix/client/r0/");
            _httpClientFactory = httpClientFactory;
        }

        public async Task<string> CreateUserAsync(string userName, string password)
        {
            var param = new MatrixRegisterRequest
            {
                UserName = userName,
                Password = password
            };

            var json = JsonConvert.SerializeObject(param);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var uri = GetUri("register", false);
            
            using var httpClient = _httpClientFactory.CreateClient();

            var response = await httpClient.PostAsync(uri, content);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"Cannot register user {userName} in matrix: {response.ReasonPhrase}");
                return response.ReasonPhrase;
            }
            return null;
        }

        public async Task<bool> UserExistsAsync(string userName, string password)
        {
            var loginResponse = await GetLoginResponse(userName, password);
            return loginResponse.IsSuccess;
        }

        public async Task<string> CheckMatrixAvailableAsync() =>
            _accessToken == null ? await Login() : null;

        private async Task<MatrixLoginResponse> GetLoginResponse(string userName, string password)
        {
            var param = new MatrixLoginRequest(userName, password);

            var json = JsonConvert.SerializeObject(param);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var uri = GetUri("login", false);
            using var httpClient = _httpClientFactory.CreateClient();

            var response = await httpClient.PostAsync(uri, content);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"Cannot login to matrix: {response.ReasonPhrase}");
                return new MatrixLoginResponse { ErrorMessage = response.ReasonPhrase};
            }

            var resultJson = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<MatrixLoginResponse>(resultJson);
        }

        private async Task<string> Login()
        {
            var loginResponse = await GetLoginResponse(_configuration.UserName, _configuration.Password);
            _accessToken = loginResponse.AccessToken;
            return loginResponse.ErrorMessage;
        }

        private Uri GetUri(string relativePath, bool useAccessToken = true)
        {
            var uri = new Uri(_baseUri, relativePath);
            
            if (useAccessToken)
                uri = new Uri(uri, $"?access_token={_accessToken}");

            return uri;
        }
    }
}
