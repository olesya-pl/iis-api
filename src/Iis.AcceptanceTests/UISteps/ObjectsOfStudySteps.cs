using System.Linq;
using AcceptanceTests.PageObjects;
using Iis.AcceptanceTests.Helpers;
using OpenQA.Selenium;
using TechTalk.SpecFlow;
using Xunit;

namespace Iis.AcceptanceTests.UISteps
{
    [Binding]
    public class ObjectsOfStudySteps
    {
        private readonly IWebDriver driver;
        private readonly ScenarioContext context;

        public ObjectsOfStudySteps(ScenarioContext injectedContext, IWebDriver driver)
        {
            context = injectedContext;
            this.driver = driver;
        }

        #region When

        [When(@"I clicked on search button in the Object of study section")]
        public void IClickedOnSearchButton()
        {
            var objectsOfStudyPage = new ObjectsOfStudyPageObjects(driver);
            objectsOfStudyPage.SearchLoopButton.Click();
        }

        [When("I clicked on enlarge small card button")]
        public void WhenIClickedOnEnlargeSmallCardButton()
        {
            var objectsOfStudyPage = new ObjectsOfStudyPageObjects(driver);
            objectsOfStudyPage.EnlargeObjectOfStudySmallCardButton.Click();
        }

        [When(@"I got search counter value in the Object of study section")]
        public void IGetSearchCounterValueInTheObjectOfStudySection()
        {
            var objectsOfStudyPage = new ObjectsOfStudyPageObjects(driver);
            context.SetResponse("counterValue", objectsOfStudyPage.OOSSearchCounter.Text);
        }

        [When(@"I clicked on first object of study")]
        public void IClickedOnFirstObjectOfStudy()
        {
            var objectOfStudyPage = new ObjectsOfStudyPageObjects(driver);
            objectOfStudyPage.TitleOfTheFirstObject.Click();
        }

        [When(@"I searched (.*) data in the Objects of study section")]
        public void IEnteredDataInTheSearchField(string input)
        {
            var objectsOfStudyPage = new ObjectsOfStudyPageObjects(driver);
            objectsOfStudyPage.SearchField.SendKeys(input);
            objectsOfStudyPage.SearchField.SendKeys(Keys.Enter);
            driver.WaitFor(7);

        }
        #endregion

        #region Then
        [Then(@"I must see object of study (.*) as first search result")]
        public void IMustSeeObjectOfStudyInTheSearchResults(string objectOfStudyTitle)
        {
            var objectOfStudyPage = new ObjectsOfStudyPageObjects(driver);
            var actualTitle = objectOfStudyPage.TitleOfTheFirstObject.Text;
            Assert.Equal(objectOfStudyTitle, actualTitle);
        }

        [Then(@"I must not see object of study (.*) as first search result")]
        public void IMustNotSeeObjectOfStudyAsFirstResult(string objectOfStudyTitle)
        {
            var objectOfStudyPage = new ObjectsOfStudyPageObjects(driver);
            var actualTitle = objectOfStudyPage.TitleOfTheFirstObject.Text;
            Assert.NotEqual(objectOfStudyTitle, actualTitle);
        }

        [Then(@"I must see person (.*) as first search result")]
        public void IMustSeePersonAsFirstSearchResult(string personTitle)
        {
            var objectOfStudyPage = new ObjectsOfStudyPageObjects(driver);
            var actualPersonTitle = objectOfStudyPage.PersonSearchResult.Text;
            Assert.Equal(personTitle, actualPersonTitle);
        }

        [Then(@"I must see search results counter value that equal to (.*) value")]
        public void IMustSeeSearchResultsThatEqualToValue(string searchResultValueInTheSearchField)
        {
            var objectOfStudyPage = new ObjectsOfStudyPageObjects(driver);
            var actualSearchResultValueInTheSearchField = objectOfStudyPage.SearchCounterInOOSSearchField.Text;
            Assert.Equal(actualSearchResultValueInTheSearchField, searchResultValueInTheSearchField);
        }

        [Then(@"I must see the object of study small card")]
        public void IMustSeeTheObjectOfStudySmallCard()
        {
            var objectOfStudyPage = new ObjectsOfStudyPageObjects(driver);
            Assert.True(objectOfStudyPage.ObjectOfStudySmallCardWindow.Displayed);
        }

        [Then(@"I must see the title (.*) in the small card")]
        public void IMustSeeTheTitleInTheSmallCard(string expectedTitle)
        {
            var objectOfStudyPage = new ObjectsOfStudyPageObjects(driver);
            var actualTitle = objectOfStudyPage.ObjectTitleInTheSmallCard.Text;
            Assert.Equal(expectedTitle, actualTitle);
        }

        [Then(@"I must see these tabs in the big object of study card")]
        public void IMustSeeTheseTabsInTheBigObjectOfStudyCard(Table table)
        {
            foreach (TableRow row in table.Rows)
            {
                var objectOfStudyPage = new ObjectsOfStudyPageObjects(driver);
                var tableValue = row.Values.First();

                Assert.True((typeof(ObjectsOfStudyPageObjects).GetField(tableValue).GetValue(objectOfStudyPage) as IWebElement).Displayed);

            }
        }

        [Then(@"I must see the specific text blocks in big object of study card")]
        public void ThenIMustSeeTheSpecificTextBlocksInBigObjectOfStudyCard(Table table)
        {
            foreach (TableRow row in table.Rows)
            {
                var objectOfStudyPage = new ObjectsOfStudyPageObjects(driver);
                var tableValue = row.Values.First();

                Assert.True((typeof(ObjectsOfStudyPageObjects).GetField(tableValue).GetValue(objectOfStudyPage) as IWebElement).Displayed);
            }
        }

        [Then(@"I must see that search counter values are equal in the Objects of study section")]
        public void IMustSeeThatSearchCounterValuesAreEqualInTheObjectOfStudySection()
        {
            var objectOfStudyPage = new ObjectsOfStudyPageObjects(driver);
            var actualValue = objectOfStudyPage.OOSSearchCounter.Text;
            var expectedValue = context.GetResponse<string>("counterValue");
            Assert.Equal(actualValue, expectedValue);
        }

        [Then(@"I must see zero search results in the Object of study page")]
        public void ThenIMustSeeZeroResults()
        {
            var objectOfStudyPage = new ObjectsOfStudyPageObjects(driver);
            Assert.True(objectOfStudyPage.OOSEmptySearchResults.Displayed);
        }

        #endregion
    }
}