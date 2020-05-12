using System.Net.Http;
using System.Threading.Tasks;
using AcceptanceTests.Environment;
using GraphQL;
using GraphQL.Client.Http;

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

            var graphQlClient = new HttpClient().AsGraphQLClient(@"http://192.168.88.70:5000");
            var response = await graphQlClient.SendMutationAsync<LoginResponse>(request);
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

            var graphQlClient = new HttpClient().AsGraphQLClient(@"http://192.168.88.70:5000");
            graphQlClient.HttpClient.DefaultRequestHeaders.Add("Authorization", authToken);
            var response = await graphQlClient.SendMutationAsync<MaterialResponse>(request);
            return response.Data;
        }

    }
}