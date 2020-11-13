using System.Linq;
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

        [When(@"I clicked on the first material in the Materials list")]
        public void IClieckedOnTheFirstMaterialInTheMaterialsList()
        {
            var materialsPage = new MaterialsPageObjects(context);
            materialsPage.FirstMaterialInTheMaterialsList.Click();
        }

        [When(@"I clicked on the events tab in the material card")]
        public void IClickedOnTheEventsTabInTheMaterialCard()
        {
            var materialsPage = new MaterialsPageObjects(context);
            materialsPage.EventsTab.Click();
        }

        [When(@"I clicked on the objects tab in the material card")]
        public void IClickedOnTheObjectsTabInTheMaterialCard()
        {
            var materialsPage = new MaterialsPageObjects(context);
            materialsPage.ObjectsTab.Click();
        }

        [When(@"I clicked on the ML tab in the material card")]
        public void IClickedOnTheMLTabInTheMaterialCard()
        {
            var materialsPage = new MaterialsPageObjects(context);
            materialsPage.MLTab.Click();
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

        [Then(@"I must see processed button in the materials card")]
        public void IMustSeeProcessedButtonOnTheMaterialCard()
        {
            var materialsPage = new MaterialsPageObjects(context);
            Assert.True(materialsPage.ProcessedButton.Displayed);
        }

        [Then(@"I must see events search in the materials card")]
        public void IMustSeeEventsSearchInTheMaterialsCard()
        {
            var materialPage = new MaterialsPageObjects(context);
            Assert.True(materialPage.EventsTabSearch.Displayed);
        }

        [Then(@"I must see objects search in the materials card")]
        public void IMustSeeObjectsSearchInTheMaterialsCard()
        {
            var materialPage = new MaterialsPageObjects(context);
            Assert.True(materialPage.ObjectsTabSearch.Displayed);
        }

        [Then(@"I must see relevance drop down in the materials card")]
        public void IMustSeeRelevanceDropDownInTheMaterialsCard()
        {
            var materialsPage = new MaterialsPageObjects(context);
            Assert.True(materialsPage.RelevanceDropDown.Displayed);
        }

        [Then(@"I must see these elements")]
        public void IMustSeeTheseDropDownElements(Table table)
        {
            foreach (TableRow row in table.Rows)
            {

                var materialPage = new MaterialsPageObjects(context);
                var tableValue = row.Values.First();

                Assert.True((typeof(MaterialsPageObjects).GetField(tableValue).GetValue(materialPage) as IWebElement).Displayed);
            }
        }

        [Then(@"I must I must see at least one user in the originator drop down menu")]
        public void IMustSeeAtLeastOneUserInTheOriginatorDropDonwMenu()
        {
            var materialPage = new MaterialsPageObjects(context);
            materialPage.Originator.Click();
            //materialPage.Originator.SendKeys(Keys.Down);
            var list = driver.FindElements(By.ClassName("el-select-dropdown__item"));
            Assert.True(list.Count() > 0);
        }

        [Then(@"I must see zero results")]
        public void ThenIMustSeeZeroResults()
        {
            var materialsPage = new MaterialsPageObjects(context);
            Assert.True(materialsPage.EmptySearchField.Displayed);
        }

        [Then(@"I must see Show button in the ML tab")]
        public void ThenIMustSeeShowButtonInTheMLTab()
        {
            var materialsPage = new MaterialsPageObjects(context);
            Assert.True(materialsPage.ShowMLResultsButton.Displayed);
        }
        #endregion
    }
}