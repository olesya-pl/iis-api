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
        private readonly HttpClient _httpClient = new HttpClient();
        private readonly Uri _baseUri;
        private bool LoggedIn => _accessToken != null;
        private readonly ILogger<MatrixService> _logger;
        public bool AutoCreateUsers => _configuration.CreateUsers;

        public MatrixService(MatrixServiceConfiguration configuration, ILoggerFactory loggerFactory)
        {
            _configuration = configuration;
            _logger = loggerFactory.CreateLogger<MatrixService>();
            _baseUri = new Uri(new Uri(_configuration.Server), "_matrix/client/r0/");
            
            Login().GetAwaiter().GetResult();
        }

        public async Task<string> CreateUserAsync(string userName, string password)
        {
            if (!LoggedIn) await Login();

            var param = new MatrixLoginRequest
            {
                user = userName,
                password = password
            };

            var json = JsonConvert.SerializeObject(param);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var uri = GetUri("register", false);

            var response = await _httpClient.PostAsync(uri, content);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"Cannot register user {userName} in matrix: {response.ReasonPhrase}");
                return response.ReasonPhrase;
            }
            return null;
        }

        public async Task<bool> UserExistsAsync(string userName)
        {
            if (!LoggedIn) Login();

            var uri = GetUri("register/available/") + $"&username={userName}";
            var response = await _httpClient.GetAsync(uri);
            return response.IsSuccessStatusCode;
        }

        public async Task<string> CheckMatrixAvailable() =>
            _accessToken == null ? await Login() : null;

        private async Task<string> Login()
        {
            var param = new MatrixLoginRequest
            {
                user = _configuration.UserName,
                password = _configuration.Password
            };
            
            var json = JsonConvert.SerializeObject(param);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var uri = GetUri("login", false);
            
            var response = await _httpClient.PostAsync(uri, content);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"Cannot login to matrix: {response.ReasonPhrase}");
                return response.ReasonPhrase;
            }

            var resultJson = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<MatrixLoginResponse>(resultJson);
            _accessToken = result.access_token;
            return null;
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
