using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using GraphQL;
using GraphQL.Client.Abstractions;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.Newtonsoft;
using Newtonsoft.Json.Linq;

namespace AcceptanceTests
{
    public static class GraphQLHttpClientFactory
    {
        public static GraphQLHttpClient CreateGraphQLHttpClient()
        {
            return new GraphQLHttpClient(@"http://192.168.88.70:5000", new NewtonsoftJsonSerializer());
        }
    }
}
