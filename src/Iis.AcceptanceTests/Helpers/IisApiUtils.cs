using System.Linq;
using System.Threading.Tasks;
using Iis.AcceptanceTests.DTO;
using GraphQL;
using IIS.Core.GraphQL.Users;
using Newtonsoft.Json.Linq; //using Iis.AcceptanceTests.Environment;

namespace Iis.AcceptanceTests.Helpers
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

            var graphQlClient = GraphQLHttpClientFactory.CreateContourGraphQLHttpClient();
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
                            id
                        }
                    }
                }"
            };

            var graphQlClient = GraphQLHttpClientFactory.CreateContourGraphQLHttpClient();
            graphQlClient.HttpClient.DefaultRequestHeaders.Add("Authorization", authToken);
            var response = await graphQlClient.SendQueryAsync<MaterialResponse>(request);
            return response.Data;
        }

        public async Task<GraphQLResponse<JObject>> UpdateMaterialImportance(string id, string importance, string authToken)
        {
            var request = new GraphQLRequest
            {
                Query =
                    @"mutation updateMaterial($input: MaterialUpdateInput!)
                    {
                        updateMaterial(input: $input)
                        {
                            id
                        }
                    }",
                Variables = new
                {
                    input = new
                    {
                        id = id,
                        importanceId = importance
                    }
                }
            };
            var graphQlClient = GraphQLHttpClientFactory.CreateContourGraphQLHttpClient();
            graphQlClient.HttpClient.DefaultRequestHeaders.Add("Authorization", authToken);
            var response = await graphQlClient.SendMutationAsync<JObject>(request);
            return response;
        }

        public void ProcessGraphQlErrors(GraphQLError[] errors)
        {
            var hasUnAuthenticatedCode = errors
                                            .Select(e => e.Extensions)
                                            .SelectMany(ext => ext.Values)
                                            .Any(value => value.ToString() == "UNAUTHENTICATED");
        }

        public async Task<EventStateListResponse> GetEventStates(int page, int pageSize, string authToken)
        {
            var request = new GraphQLRequest
            {
                Query =
                    @"query{
                    entityEventStateList(pagination:{pageSize:" + pageSize + @", page:" + page + @"}){
                        count
                        items{
                            id
                            name
                        }
                    }
                }"
            };

            var graphQlClient = GraphQLHttpClientFactory.CreateContourGraphQLHttpClient();
            graphQlClient.HttpClient.DefaultRequestHeaders.Add("Authorization", authToken);
            var response = await graphQlClient.SendQueryAsync<EventStateListResponse>(request);
            return response.Data;
        }

    }
}