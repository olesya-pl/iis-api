﻿using System;
using System.Text;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Collections.Generic;
using Serilog;
using Iis.OntologyManager.DTO;
using Iis.OntologyManager.Dictionaries;
using Iis.OntologyManager.Configurations;
using Iis.Interfaces.AccessLevels;
using Iis.Services.Contracts.Params;
using System.Linq;
using Newtonsoft.Json;

namespace Iis.OntologyManager.Helpers
{
    public class RequestWraper
    {
        private readonly UserCredentials _userCredentials;
        private readonly Uri _baseApiApiAddress;
        private readonly ILogger _logger;
        private readonly RequestSettings _requestSettings;
        private readonly static IReadOnlyDictionary<IndexKeys, string> IndexPaths = new Dictionary<IndexKeys, string>
        {
            { IndexKeys.Ontology, IndexRelativePaths.Ontology },
            { IndexKeys.OntologyHistorical, IndexRelativePaths.OntologyHistorical },
            { IndexKeys.Signs, IndexRelativePaths.Signs },
            { IndexKeys.Events, IndexRelativePaths.Events },
            { IndexKeys.Reports, IndexRelativePaths.Reports },
            { IndexKeys.Materials, IndexRelativePaths.Materials},
            { IndexKeys.Wiki, IndexRelativePaths.Wiki },
            { IndexKeys.WikiHistorical, IndexRelativePaths.WikiHistorical },
            { IndexKeys.Users, IndexRelativePaths.Users },
        };

        public RequestWraper(string apiAddress, UserCredentials userCredentials, RequestSettings requestSettings, ILogger logger)
        {
            _baseApiApiAddress = new Uri(apiAddress);
            _userCredentials = userCredentials;
            _logger = logger;
            _requestSettings = requestSettings;
        }

        private HttpResponseMessage BadGatewayResponseMessage(Uri baseAddress, Uri relativeAddress)
        {
            return new HttpResponseMessage
            { 
                StatusCode = System.Net.HttpStatusCode.BadGateway,
                ReasonPhrase = "Адреса не відповідає",
                RequestMessage = new HttpRequestMessage
                {
                    RequestUri = new Uri(baseAddress, relativeAddress)
                }
            };
        }

        public Task<RequestResult> DeleteEntityAsync(Guid entityId) 
        {
            var requestUri = GetDeleteEntityRequestUri(entityId);

            return SendDeleteRequestAsync(requestUri);
        }

        public async Task RestartIisAppAsync()
        {
            var uri = "admin/RestartApplication";
            using var httpClient = GetClient(_baseApiApiAddress, _requestSettings);
            await httpClient.PostAsync(uri, null).ConfigureAwait(false);
        }
        public async Task<RequestResult> ReloadOntologyDataAsync()
        {
            var uri = new Uri("admin/ReloadOntologyData", UriKind.Relative);

            using var httpClient = GetClient(_baseApiApiAddress, _requestSettings);

            return await SendRequest(() => httpClient.PostAsync(uri, null), uri);
        }

        public async Task<RequestResult> ReIndexAsync(IndexKeys indexKey)
        {
            var pathFound = IndexPaths.TryGetValue(indexKey, out string requestUrl);

            if (!pathFound) return RequestResult.Fail("Шлях для перебудови індекса не знайдено.", new Uri("about:blank"));

            var uri = new Uri($"admin/{requestUrl}", UriKind.Relative);

            using var httpClient = GetClient(_baseApiApiAddress, _requestSettings);

            return await SendRequest(() => httpClient.GetAsync(uri), uri);
        }

        public async Task<RequestResult> ChangeAccessLevelsAsync(ChangeAccessLevelsParams param)
        {
            var uri = new Uri("admin/ChangeAccessLevels", UriKind.Relative);

            using var httpClient = GetClient(_baseApiApiAddress, _requestSettings);

            var json = JsonConvert.SerializeObject(param);

            return await SendRequest(() => httpClient.PostAsync(uri, new StringContent(json)), uri);
        }

        private async Task<RequestResult> SendRequest(Func<Task<HttpResponseMessage>> func, Uri uri)
        {
            HttpResponseMessage response = BadGatewayResponseMessage(_baseApiApiAddress, uri);

            try
            {
                response = await func().ConfigureAwait(false);

                response.EnsureSuccessStatusCode();

                var msg = response.Content.ReadAsStringAsync().Result;
                if (string.IsNullOrEmpty(msg)) msg = response.ReasonPhrase;

                return RequestResult.Success(msg, response.RequestMessage.RequestUri);
            }
            catch (Exception exception)
            {
                var msg = response.Content.ReadAsStringAsync().Result;
                _logger.Error($"Uri:{response.RequestMessage.RequestUri} Exception:{exception}");

                return RequestResult.Fail($"Code={response.StatusCode}:{response.ReasonPhrase}:{msg}", response.RequestMessage.RequestUri);
            }
        }

        private async Task<RequestResult> SendDeleteRequestAsync(Uri requestUri)
        {
            HttpResponseMessage response = BadGatewayResponseMessage(_baseApiApiAddress, requestUri);

            using var httpClient = GetClient(_baseApiApiAddress, _requestSettings);

            AddAuthorizationHeader(httpClient, _userCredentials.UserName, _userCredentials.Password);

            try
            {
                response = await httpClient.DeleteAsync(requestUri).ConfigureAwait(false);

                response.EnsureSuccessStatusCode();

                return RequestResult.Success("Сутність видалено", response.RequestMessage.RequestUri);
            }
            catch (Exception exception)
            {
                _logger.Error($"Uri:{response.RequestMessage.RequestUri} Exception:{exception}");

                return RequestResult.Fail($"Code={response.StatusCode}:{response.ReasonPhrase}", response.RequestMessage.RequestUri);
            }
        }

        private static HttpClient GetClient(Uri apiAddress, RequestSettings requestSettings)
        {
            var client = new HttpClient
            {
                BaseAddress = apiAddress,
                Timeout = TimeSpan.FromMinutes(requestSettings.ReIndexTimeOutInMins)
            };

            return client;
        }

        private static HttpClient AddAuthorizationHeader(HttpClient client, string userName, string userPassword)
        {
            var byteArray = Encoding.ASCII.GetBytes($"{userName}:{userPassword}");

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

            return client;
        }

        private static Uri GetDeleteEntityRequestUri(Guid entityId)
        {
            return new Uri($"api/entity/{entityId:N}", UriKind.Relative);
        }
    }
}
