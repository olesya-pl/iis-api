﻿using System;
using System.Text;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Collections.Generic;
using Serilog;
using Iis.Services.Contracts.Params;
using Newtonsoft.Json;
using Iis.Desktop.Common.Login;
using Iis.Interfaces.SecurityLevels;

namespace Iis.Desktop.Common.Requests
{
    public class RequestWraper
    {
        private readonly UserCredentials _userCredentials;
        private readonly Uri _baseApiApiAddress;
        private readonly ILogger _logger;
        private readonly RequestSettings _requestSettings;
        private readonly static IReadOnlyDictionary<IndexKeys, string> IndexPaths = new Dictionary<IndexKeys, string>
        {
            {IndexKeys.Ontology, ApiRouteList.OntologyReIndex},
            {IndexKeys.Signs, ApiRouteList.SignsReIndex},
            {IndexKeys.Events, ApiRouteList.EventsReIndex},
            {IndexKeys.Reports, ApiRouteList.ReportsReIndex},
            {IndexKeys.Materials, ApiRouteList.MaterialsReIndex},
            {IndexKeys.Wiki, ApiRouteList.WikiReIndex},
            {IndexKeys.Users, ApiRouteList.UsersReIndex},
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
            using var httpClient = GetClient(_baseApiApiAddress, _requestSettings);
            await httpClient.PostAsync(ApiRouteList.ApplicationRestart, null).ConfigureAwait(false);
        }
        public async Task<RequestResult> ReloadOntologyDataAsync()
        {
            var uri = new Uri(ApiRouteList.OntologyReloadData, UriKind.Relative);

            using var httpClient = GetClient(_baseApiApiAddress, _requestSettings);

            return await SendRequestAsync(() => httpClient.PostAsync(uri, null), uri);
        }

        public async Task<RequestResult> ReIndexAsync(IndexKeys indexKey)
        {
            var pathFound = IndexPaths.TryGetValue(indexKey, out string requestUrl);

            if (!pathFound) return RequestResult.Fail("Шлях для перебудови індекса не знайдено.", new Uri("about:blank"));

            var uri = new Uri(requestUrl, UriKind.Relative);

            using var httpClient = GetClient(_baseApiApiAddress, _requestSettings);

            return await SendRequestAsync(() => httpClient.GetAsync(uri), uri).ConfigureAwait(false);
        }

        public async Task<RequestResult> ChangeAccessLevelsAsync(ChangeAccessLevelsParams param)
        {
            var uri = new Uri(ApiRouteList.AccessLevelChange, UriKind.Relative);

            using var httpClient = GetClient(_baseApiApiAddress, _requestSettings);

            var json = JsonConvert.SerializeObject(param);

            return await SendRequestAsync(() => httpClient.PostAsync(uri, new StringContent(json, Encoding.UTF8, "application/json")), uri);
        }

        public async Task<IReadOnlyList<SecurityLevelPlain>> GetSecurityLevels()
        {
            var uri = new Uri(ApiRouteList.GetSecurityLevels, UriKind.Relative);

            using var httpClient = GetClient(_baseApiApiAddress, _requestSettings);

            var response = await SendRequestAsync(() => httpClient.GetAsync(uri), uri).ConfigureAwait(false);

            var result = response.IsSuccess ?
                JsonConvert.DeserializeObject<List<SecurityLevelPlain>>(response.Message) :
                new List<SecurityLevelPlain>();

            return result;
        }

        public async Task<IReadOnlyList<UserSecurityDto>> GetUserSecurityDtos()
        {
            var uri = new Uri(ApiRouteList.GetUserSecurityDtos, UriKind.Relative);

            using var httpClient = GetClient(_baseApiApiAddress, _requestSettings);

            var response = await SendRequestAsync(() => httpClient.GetAsync(uri), uri).ConfigureAwait(false);

            var result = response.IsSuccess ?
                JsonConvert.DeserializeObject<List<UserSecurityDto>>(response.Message) :
                new List<UserSecurityDto>();

            return result;
        }

        public async Task SaveUserSecurityDto(UserSecurityDto userSecurityDto)
        {
            var uri = new Uri(ApiRouteList.SaveUserSecurityDto, UriKind.Relative);

            using var httpClient = GetClient(_baseApiApiAddress, _requestSettings);

            var json = JsonConvert.SerializeObject(userSecurityDto);

            var response = await SendRequestAsync(() => httpClient.PostAsync(uri, new StringContent(json, Encoding.UTF8, "application/json")), uri)
                .ConfigureAwait(false);
        }

        public async Task<ObjectSecurityDto> GetObjectSecurityDtos(Guid id)
        {
            var uri = new Uri(ApiRouteList.GetObjectSecurityDtos, UriKind.Relative);

            using var httpClient = GetClient(_baseApiApiAddress, _requestSettings);

            var response = await SendRequestAsync(() => httpClient.GetAsync($"{uri}/{id}"), uri).ConfigureAwait(false);

            var result = response.IsSuccess ?
                JsonConvert.DeserializeObject<ObjectSecurityDto>(response.Message) :
                new ObjectSecurityDto { Title = String.Empty, SecurityIndexes = new List<int>() };

            return result;
        }

        public async Task SaveObjectSecurityDto(ObjectSecurityDto objectSecurityDto)
        {
            var uri = new Uri(ApiRouteList.SaveObjectSecurityDto, UriKind.Relative);

            using var httpClient = GetClient(_baseApiApiAddress, _requestSettings);

            var json = JsonConvert.SerializeObject(objectSecurityDto);

            var response = await SendRequestAsync(() => httpClient.PostAsync(uri, new StringContent(json, Encoding.UTF8, "application/json")), uri)
                .ConfigureAwait(false);
        }

        private async Task<RequestResult> SendRequestAsync(Func<Task<HttpResponseMessage>> func, Uri uri)
        {
            HttpResponseMessage response = BadGatewayResponseMessage(_baseApiApiAddress, uri);

            try
            {
                response = await func().ConfigureAwait(false);

                response.EnsureSuccessStatusCode();

                var msg = await response.Content?.ReadAsStringAsync();
                if (string.IsNullOrEmpty(msg)) msg = response.ReasonPhrase;

                return RequestResult.Success(msg, response.RequestMessage.RequestUri);
            }
            catch (Exception ex)
            {
                _logger.Error($"Uri:{uri} Exception:{ex}");

                if (response == null)
                {
                    return RequestResult.Fail(ex.Message, uri);
                }
                var msg = response.Content == null ? null : await response.Content.ReadAsStringAsync();
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
            var byteArray = Encoding.UTF8.GetBytes($"{userName}:{userPassword}");

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

            return client;
        }

        private static Uri GetDeleteEntityRequestUri(Guid entityId)
        {
            return new Uri($"api/entity/{entityId:N}", UriKind.Relative);
        }
    }
}
