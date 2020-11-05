using System.Linq;
using System.Threading.Tasks;
using Iis.AcceptanceTests.DTO;
using Iis.AcceptanceTests.Helpers;
using FluentAssertions;
using GraphQL;
using Newtonsoft.Json.Linq;
using TechTalk.SpecFlow;

namespace Iis.AcceptanceTests.APISteps
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

        [When(@"I want to set importance (.*) for the first material on page")]
        public async Task WhenIWantToSetImportanceForTheFirstMaterialOnPage(string importance)
        {
            var materialResponse = await IisApiUtils.GetMaterials(1, 1, context.GetAuthToken());
            var materialId = ((JContainer)materialResponse.Materials["items"]).First()["id"].ToString();
            context.SetResponse(
                context.ScenarioInfo.Title,
                await IisApiUtils.UpdateMaterialImportance(materialId, importance, context.GetAuthToken()));
        }

        [Then(@"the result should contain (.*) items in the response")]
        public void ThenTheResultShouldContainItemsInTheResponse(int itemsCount)
        {
            var materialResponse = context.GetResponse<MaterialResponse>(context.ScenarioInfo.Title);
            materialResponse.Materials.Value<int>("count").Should().BePositive();
            ((JContainer) materialResponse.Materials["items"]).Count.Should().Be(itemsCount);
        }

        [Then(@"update should be executed without errors")]
        public void ThenUpdateIsExecutedWithoutErrors()
        {
            var updateResponse = context.GetResponse<GraphQLResponse<JObject>>(context.ScenarioInfo.Title);
            updateResponse.Errors.Should().BeNull();
        }
    }
}
