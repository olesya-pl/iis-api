using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using GraphQL;
using GraphQL.Client.Http;
using IIS.Core.GraphQL.Users;
using TechTalk.SpecFlow;
using Xunit;

namespace AcceptanceTests.Steps
{
    [Binding]
    public sealed class AuthorizationSteps : IisApiUtils
    {
        // For additional details on SpecFlow step definitions see https://go.specflow.org/doc-stepdef

        private readonly ScenarioContext context;
        private readonly GraphQLHttpClient graphQlClient;
        private const string authResponce = "authResponse";
        private const string errResponce = "Wrong username or password";

        public AuthorizationSteps(ScenarioContext injectedContext)
        {
            context = injectedContext;
            graphQlClient = GraphQLHttpClientFactory.CreateContourGraphQLHttpClient();
        }

        [Given(@"I want to authenticate with the user (.*) and password (.*) in the Contour")]
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

        [When(@"I send login request to the Contour")]
        public async Task WhenWhenISendLoginRequest()
        {
            var authRequest = context.Get<GraphQLRequest>("authRequest");
            //var response = await graphQlClient.SendMutationAsync<GraphQlResponseWrapper<LoginResponse>>(authRequest);
            var response = await graphQlClient.SendMutationAsync<GraphQlResponseWrapper<LoginResponse>>(authRequest);
            context.Add(authResponce, response);
        }

        [Then(@"The result should contain authorization token from the Countour")]
        public void ThenTheResultShouldBeStatusCode()
        {
            var response = context.Get<GraphQLResponse<GraphQlResponseWrapper<LoginResponse>>>("authResponse");
            Assert.NotNull(response.Data.Login.Token);
        }

        [Then(@"The result should not contain authorization token from the Contour")]
        public void ThenTheResultShouldNotContainAuthorizationToken()
        {
            var response = context.Get<GraphQLResponse<GraphQlResponseWrapper<LoginResponse>>>("authResponse");
            response.Data.Login.Should().BeNull();
            response.Errors.Any(_ => _.Message == "Wrong username or password").Should().BeTrue();
        }
    }
}




