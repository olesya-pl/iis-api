using Iis.AcceptanceTests.Helpers;
using Iis.AcceptanceTests.PageObjects;
using OpenQA.Selenium;
using TechTalk.SpecFlow;
using Xunit;

namespace Iis.AcceptanceTests.UISteps
{
    [Binding]
    public class MaterialsSteps
    {
        private readonly IWebDriver driver;
        private readonly ScenarioContext context;

        public MaterialsSteps(ScenarioContext injectedContext)
        {
            context = injectedContext;
            driver = context.GetDriver();
        }

        #region When
        [When(@"I navigated to Materials page")]
        public void IWantNavigateToMaterialsPage()
        {
            var materialsPage = new MaterialsPageObjects(context);
            materialsPage.MaterialsSection.Click();

            driver.WaitFor(10);
        }

        [When(@"I clicked Search button")]
        public void IFilledInfoInTheMaterialsSearchField()
        {
            var materialsPage = new MaterialsPageObjects(context);
            materialsPage.SearchButton.Click();
        }

        [When(@"I entered (.*) data in the search field")]

        public void IEnteredDataInTheSearchField(string input)
        {
            var materialsPage = new MaterialsPageObjects(context);
            materialsPage.SearchField.SendKeys(input);
            materialsPage.SearchField.SendKeys(Keys.Enter);
        }
        #endregion When

        #region Then
        [Then(@"I must see the Materials page")]
        public void ThenIMustSeeMaterialsPage()
        {
            Assert.Contains("input-stream/?page=1", driver.Url);
        }

        [Then(@"I must see first material in the Materials list")]
        public void ThenIMustSeeFirstMaterialInTheMaterialsList()
        {
            var materialsPage = new MaterialsPageObjects(context);
            Assert.True(materialsPage.FirstMaterialInTheMaterialsList.Displayed);
        }

        [Then(@"I must see zero results")]
        public void ThenIMustSeeZeroResults()
        {
            var materialsPage = new MaterialsPageObjects(context);
            Assert.True(materialsPage.EmptySearchField.Displayed);
        }
        #endregion
    }
}