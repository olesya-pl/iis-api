using System.Net.Http;
using System.Threading.Tasks;
using AcceptanceTests.Environment;
using GraphQL;
using GraphQL.Client.Http;
using TechTalk.SpecFlow;
using Xunit;

namespace AcceptanceTests.Steps
{
    [Binding]
    public sealed class AuthorizationSteps
    {
        // For additional details on SpecFlow step definitions see https://go.specflow.org/doc-stepdef

        private readonly ScenarioContext context;
        private readonly GraphQLHttpClient graphQlClient;

        public AuthorizationSteps(ScenarioContext injectedContext)
        {
            context = injectedContext;
            graphQlClient = new HttpClient().AsGraphQLClient(@"http://192.168.88.70:5000");
        }

        [Given(@"I want to authenticate with the user (.*) and password (.*)")]
        public void GivenIWantToAuthenticateWithTheUserAndPassword(string userName, string password)
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
            context.Add("authRequest", request);
        }

        [When(@"When I send login request")]
        public async Task WhenWhenISendLoginRequest()
        {
            var authRequest = context.Get<GraphQLRequest>("authRequest");
            var response = await graphQlClient.SendMutationAsync<LoginResponse>(authRequest);
            context.Add("authResponse", response);
        }

        [Then(@"The result should contain authorization token")]
        public void ThenTheResultShouldBeStatusCode()
        {
            var response = context.Get<GraphQLResponse<LoginResponse>>("authResponse");
            Assert.NotNull(response.Data.Login.Token);
        }

    }
}
