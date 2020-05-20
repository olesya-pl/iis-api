using System;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using GraphQL.Client.Http;
using Newtonsoft.Json.Linq;
using TechTalk.SpecFlow;

namespace AcceptanceTests.Steps
{
    [Binding]
    public class MaterialsSteps
    {
        private readonly ScenarioContext context;
        private readonly IisApiUtils IisApiUtils;

        public MaterialsSteps(ScenarioContext injectedContext)
        {
            context = injectedContext;
            IisApiUtils = new IisApiUtils();
        }

        [Given(@"Authorized user with login (.*) and password (.*)")]
        public async Task GivenAuthorizedUserWithLoginOlyaAndPasswordHammer(string userName, string password)
        {
            context.SetAuthToken(await IisApiUtils.Login(userName, password));
        }

        [When(@"I want to request materials for page (.*) and get (.*) materials per page")]
        public async Task WhenIWantToRequestMaterialsFroPageAndGetMaterialsPerPage(int page, int pageSize)
        {
            context.SetResponse(context.ScenarioInfo.Title, await IisApiUtils.GetMaterials(page, pageSize, context.GetAuthToken()));
        }

        [Then(@"the result should contain (.*) items in the response")]
        public void ThenTheResultShouldContainItemsInTheResponse(int itemsCount)
        {
            var materialResponse = context.GetResponse<MaterialResponse>(context.ScenarioInfo.Title);
            materialResponse.Materials.Value<int>("count").Should().BePositive();
            ((JContainer) materialResponse.Materials["items"]).Count.Should().Be(itemsCount);
        }
    }
}
