﻿using GraphQL;
using GraphQL.Client.Http;
using System;
using System.Linq;
using System.Net.Http;
using System.Security.Authentication;
using System.Threading;
using System.Threading.Tasks;

namespace Iis.ApiTests
{
    abstract public class BaseAPITest
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

            var errorMessage = string.Join(Environment.NewLine, errorMessages);

            if (hasUnAuthenticatedCode)
            {
                throw new AuthenticationException(errorMessage);
            }
            else
            {
                throw new InvalidOperationException(errorMessage);
            }

        }

        protected async Task<(bool state, string token, string exceptionMessage)> Login(string username,
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
                using var graphQlClient = new HttpClient().AsGraphQLClient(uri);


                var response = await graphQlClient.SendMutationAsync<LoginResponse>(request, cancellationToken);

                if (response.Errors != null && response.Errors.Any())
                {
                    ProcessGraphQlErrors(response.Errors);
                }

                return (true, response.Data.Login.Token, null);
            }

            catch (Exception ex)
            {
                return (false, null, ex.Message);
            }
        }
    }
}
