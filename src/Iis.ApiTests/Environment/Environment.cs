using GraphQL;
using GraphQL.Client.Http;
using System;
using System.Linq;
using System.Net.Http;
using System.Security.Authentication;
using System.Threading;

namespace Iis.ApiTests
{
    abstract public class Environment
    {
        public class LoginResponse
        {
            public UserLogin Login { get; set; }

            public class UserLogin
            {
                public string Token { get; set; }
            }
        }

        private void ProcessGraphQlErrors(GraphQLError[] errors)
        {
            var hasUnAuthenticatedCode = errors
                                            .Select(e => e.Extensions)
                                            .SelectMany(ext => ext.Values)
                                            .Any(value => value.ToString() == "UNAUTHENTICATED");
            var errorMessages = errors
                                .Select(e => e.Message)
                                .ToArray();

            var errorMessage = string.Join(System.Environment.NewLine, errorMessages);

            if (hasUnAuthenticatedCode)
            {
                throw new AuthenticationException(errorMessage);
            }
            else
            {
                throw new InvalidOperationException(errorMessage);
            }

        }
        protected TResponce ExecureGraphQlRequest<TResponce>(string apiUri, GraphQLRequest request, string authToken = null, CancellationToken cancellationToken = default)
            where TResponce : class
        {
            using var httpClient = new HttpClient();
            if(!string.IsNullOrWhiteSpace(authToken))
                httpClient.DefaultRequestHeaders.Add("Authorization", authToken);

            using var graphQlClient = httpClient.AsGraphQLClient(apiUri);
            var response = graphQlClient.SendMutationAsync<TResponce>(request, cancellationToken).GetAwaiter().GetResult();

            if (response.Errors != null && response.Errors.Any())
            {
                ProcessGraphQlErrors(response.Errors);
            }

            return response.Data;
        }
        protected (bool state, string token, string exceptionMessage) Login(string username,
            string password, string uri, CancellationToken cancellationToken = default)
        {
            
            var request = new GraphQLRequest
            {
                Query =
                @"
                    mutation Login($u: String!, $p: String!)
                    {
                        login(username:$u, password:$p)
                        {
                            token
                        }
                    }
                ",
                Variables = new
                {
                    u = username,
                    p = password
                }
            };



            try
            {
                var response = ExecureGraphQlRequest<LoginResponse>(uri, request, null, cancellationToken);

                return (true, response.Login.Token, null);
            }

            catch (Exception ex)
            {
                return (false, null, ex.Message);
            }
        }
    }
}
