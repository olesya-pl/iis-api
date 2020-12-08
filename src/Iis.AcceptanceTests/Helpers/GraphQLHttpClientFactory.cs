﻿using GraphQL.Client.Http;
using GraphQL.Client.Serializer.Newtonsoft;

namespace AcceptanceTests.Helpers
{
    public static class GraphQLHttpClientFactory
    {
        public static GraphQLHttpClient CreateContourGraphQLHttpClient()
        {
            return new GraphQLHttpClient(@"http://192.168.88.70:5000", new NewtonsoftJsonSerializer());
        }

        public static GraphQLHttpClient CreateOdysseusGraphQLHttpClient()
        {
            return new GraphQLHttpClient(@"http://192.168.88.65:5000", new NewtonsoftJsonSerializer());
        }

        public static GraphQLHttpClient WithAuthToken(this GraphQLHttpClient graphQLClient, string authToken)
        {
            graphQLClient.HttpClient.DefaultRequestHeaders.Remove("Authorization");

            graphQLClient.HttpClient.DefaultRequestHeaders.Add("Authorization", authToken);

            return graphQLClient;
        }
    }
}
