using Iis.Desktop.Common.Requests;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Iis.Desktop.Common.GraphQL
{
    public class GraphQLRequestWrapper<TParam, TResult>
    {
        private const string ErrorsName = "errors";
        private const string MessageName = "message";
        private const string DataName = "data"; 
        private const string StackTraceName = "extensions.stackTrace";
        private Uri _serverUri;

        public GraphQLRequestWrapper(string urlString)
        {
            _serverUri = new Uri(urlString);
        }
        public GraphQLRequestWrapper(Uri uri)
        {
            _serverUri = uri;
        }

        public async Task<GraphQLResponse<TResult>> Send(string query, TParam param, string operationName = null)
        {
            var request = new GraphQLRequest<TParam>
            {
                OperationName = operationName,
                Variables = param,
                Query = query
            };

            using (var httpClient = GetHttpClient())
            {
                var json = JsonConvert.SerializeObject(request, new JsonSerializerSettings()
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                });
                var response = await httpClient.PostAsync(_serverUri, new StringContent(json, Encoding.UTF8, "application/json"));
                var body = await response.Content.ReadAsStringAsync();
                var bodyObject = JObject.Parse(body);
                if (bodyObject.ContainsKey(ErrorsName))
                {
                    return new GraphQLResponse<TResult>(default, ParseError((JArray)bodyObject[ErrorsName]));
                }
                else
                {
                    var data = (JObject)bodyObject[DataName][operationName];
                    var result = data.ToObject<TResult>();
                    return new GraphQLResponse<TResult>(result, null);
                }
            }
        }

        private GraphQLError ParseError(JArray errors)
        {
            var result = new GraphQLError { Message = errors.First[MessageName].ToString() };
            return result;
        }
        private HttpClient GetHttpClient() => new HttpClient { BaseAddress = _serverUri };
    }
}
