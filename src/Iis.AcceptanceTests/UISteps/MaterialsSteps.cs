using System.Linq;
using AcceptanceTests.PageObjects;
using Iis.AcceptanceTests.Helpers;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using TechTalk.SpecFlow;
using Xunit;

namespace Iis.AcceptanceTests.UISteps
{
    [Binding]
    public class MaterialsSteps
    {
        private readonly IWebDriver driver;
        private readonly ScenarioContext context;
        private MaterialsSectionPage materialsSectionPage;

        public MaterialsSteps(ScenarioContext injectedContext, IWebDriver driver)
        {
            materialsSectionPage = new MaterialsSectionPage(driver);

            context = injectedContext;
            this.driver = driver;
        }

        #region When
        [When(@"I navigated to Materials page")]
        public void IWantNavigateToMaterialsPage()
        {
            materialsSectionPage.MaterialsSection.Click();

            driver.WaitFor(10);
        }

        [When(@"I clicked on the first material in the Materials list")]
        public void IClieckedOnTheFirstMaterialInTheMaterialsList()
        {
            materialsSectionPage.FirstMaterialInTheMaterialsList.Click();
        }

        [When(@"I clicked on the events tab in the material card")]
        public void IClickedOnTheEventsTabInTheMaterialCard()
        {
            materialsSectionPage.EventsTab.Click();
        }

        [When(@"I clicked on the objects tab in the material card")]
        public void IClickedOnTheObjectsTabInTheMaterialCard()
        {
            materialsSectionPage.ObjectsTab.Click();
        }

        [When(@"I clicked on the ML tab in the material card")]
        public void IClickedOnTheMLTabInTheMaterialCard()
        {
            materialsSectionPage.MLTab.Click();
        }

        [When(@"I got search counter value in the Materials section")]
        public void IGetSearchCounterValueInTheMaterialsSection()
        {
            context.SetResponse("counterValue", materialsSectionPage.MaterialsSearchResultCounter.Text);
        }

        [When(@"I clicked search button in the Materials section")]
        public void IFilledInfoInTheMaterialsSearchField()
        {
            materialsSectionPage.SearchButton.Click();
        }

        [When(@"I searched (.*) data in the materials")]
        public void IEnteredDataInTheSearchField(string input)
        {
            materialsSectionPage.SearchField.SendKeys(input);
            driver.WaitFor(2);
            materialsSectionPage.SearchField.SendKeys(Keys.Enter);
        }

        [When(@"I set importance (.*) value")]
        public void WhenIClickedOnTheDropDownMenuInTheMaterials(string value)
        {
            materialsSectionPage.MaterialPage.ImportanceDropDown.Select(value);
        }

        [When(@"I set relevance (.*) value")]
        public void WhenISetPriority(string priority)
        {
            materialsSectionPage.MaterialPage.RelevanceDropDown.Select(priority);
        }


        [When(@"I pressed Processed button")]
        public void WhenIPressProcessedButton()
        {
            materialsSectionPage.ProcessedButton.Click();
        }

        [When(@"I pressed Back button")]
        public void WhenIPressBackButton()
        {
            materialsSectionPage.BackButton.Click();
        }

        [When(@"I clicked on the first search result in the Materials section")]
        public void WhenIClickedOnTheFirstSearchResultInTheMaterialsSection()
        {
            materialsSectionPage.FirstSearchResult.Click();
        }

        [When(@"I pressed Show button to show Text classifier ML output")]
        public void WhenIPressedShowButtonToShowTextClassifierMLOutput()
        {
            materialsSectionPage.TextClassifierMLOutputButton.Click();
        }

        [When(@"I enter (.*) value in the search object field")]
        public void WhenIEnterValueInTheSearchObjectField(string inputValue)
        {
            materialsSectionPage.ObjectsTabSearch.SendKeys(inputValue);
            driver.WaitFor(2);
            materialsSectionPage.ObjectsTabSearch.SendKeys(Keys.Down);
            materialsSectionPage.ObjectsTabSearch.SendKeys(Keys.Enter);
        }

        [When(@"I clicked on the connected object")]
        public void WhenIClickedOnTheConnectedObject()
        {
            materialsSectionPage.ConnectedObjectLink.Click();
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

            Assert.True(materialsSectionPage.FirstMaterialInTheMaterialsList.Displayed);
        }

        [Then(@"I must see processed button in the materials card")]
        public void IMustSeeProcessedButtonOnTheMaterialCard()
        {

            Assert.True(materialsSectionPage.ProcessedButton.Displayed);
        }

        [Then(@"I must see events search in the materials card")]
        public void IMustSeeEventsSearchInTheMaterialsCard()
        {

            Assert.True(materialsSectionPage.EventsTabSearch.Displayed);
        }

        [Then(@"I must see objects search in the materials card")]
        public void IMustSeeObjectsSearchInTheMaterialsCard()
        {
            Assert.True(materialsSectionPage.ObjectsTabSearch.Displayed);
        }

        [Then(@"I must see relevance drop down in the materials card")]
        public void IMustSeeRelevanceDropDownInTheMaterialsCard()
        {
            Assert.True(materialsSectionPage.RelevanceDropDown.Displayed);
        }

        [Then(@"I must see these elements")]
        public void IMustSeeTheseDropDownElements(Table table)
        {
            foreach (TableRow row in table.Rows)
            {
                var tableValue = row.Values.First();
                Assert.True((typeof(MaterialsSectionPage).GetField(tableValue).GetValue(materialsSectionPage) as IWebElement).Displayed);
            }
        }

        [Then(@"I must I must see at least one user in the originator drop down menu")]
        public void IMustSeeAtLeastOneUserInTheOriginatorDropDonwMenu()
        {
            materialsSectionPage.Originator.Click();
            var list = driver.FindElements(By.ClassName("el-select-dropdown__item"));
            Assert.True(list.Count() > 0);
        }

        [Then(@"I must see zero results in the Materials section")]
        public void ThenIMustSeeZeroResults()
        {
            Assert.True(materialsSectionPage.EmptySearchField.Displayed);
        }

        [Then(@"I must see Show button in the ML tab")]
        public void ThenIMustSeeShowButtonInTheMLTab()
        {
            Assert.True(materialsSectionPage.ShowMLResultsButton.Displayed);
        }

        [Then(@"I must see that search counter values are equal in the Materials section")]
        public void ThenIMustSeeThatSearchCounterValuesAreEqualInTheMaterialsSection()
        {
            var actualValue = materialsSectionPage.MaterialsSearchResultCounter.Text;
            var expectedValue = context.GetResponse<string>("counterValue");
            Assert.Equal(actualValue, expectedValue);
        }

        [Then(@"I must see that importance value must be set to (.*) value")]
        public void ThemIMustSeeThatImportanceValueMustBeSetToValue(string expectedValue)
        {
            var actualValue = materialsSectionPage.MaterialPage.ImportanceDropDown.Text;
            Assert.Equal(expectedValue, actualValue);
        }

        [Then(@"I must see Text classifier ML output form")]
        public void IMustSeeTextClassifierMLOutPutFor()
        {
            Assert.True(materialsSectionPage.TextClassifierMLOutputForm.Displayed);
        }

        [Then(@"I must see a material that contains (.*) word in the Materials search result")]
        public void ThenIMustSeeAMaterialThatContainsWordInTheMaterialSearchResult(string keyword)
        {
            var actualContentText = materialsSectionPage.FirstSearchResultContentBlock.Text;
            Assert.Contains(keyword, actualContentText);
        }
        #endregion
    }
}