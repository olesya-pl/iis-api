using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
//using AcceptanceTests.Environment;
using GraphQL;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.Newtonsoft;
using IIS.Core.GraphQL.Users;

namespace AcceptanceTests.Steps
{
    public class IisApiUtils
    {
        public async Task<string> Login(string userName, string password)
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
                    u = userName,
                    p = password
                }
            };

            var graphQlClient = GraphQLHttpClientFactory.CreateGraphQLHttpClient();
            var response = await graphQlClient.SendMutationAsync<GraphQlResponseWrapper<LoginResponse>>(request);
            return response.Data.Login.Token;
        }

        public async Task<MaterialResponse> GetMaterials(int page, int pageSize, string authToken)
        {
            var request = new GraphQLRequest
            {
                Query =
                    @"query{
                    materials(pagination:{pageSize:" + pageSize + @", page:" + page + @"}){
                        count
                        items{
                            id,
                            metadata
                            {
                                type,
                                source
                            }
                        }
                    }
                }"
            };

            var graphQlClient = GraphQLHttpClientFactory.CreateGraphQLHttpClient();
            graphQlClient.HttpClient.DefaultRequestHeaders.Add("Authorization", authToken);
            var response = await graphQlClient.SendMutationAsync<MaterialResponse>(request);
            return response.Data;
        }

        public void ProcessGraphQlErrors(GraphQLError[] errors)
        {
            var hasUnAuthenticatedCode = errors
                                            .Select(e => e.Extensions)
                                            .SelectMany(ext => ext.Values)
                                            .Any(value => value.ToString() == "UNAUTHENTICATED");
        }

    }
}