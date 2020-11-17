using Iis.AcceptanceTests.Helpers;
using Iis.AcceptanceTests.PageObjects;
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

        public ObjectsOfStudySteps(ScenarioContext injectedContext)
        {
            context = injectedContext;
            driver = context.GetDriver();
        }

        #region When

        [When(@"I clicked on search button")]
        public void IClickedOnSearchButton()
        {
            var objectsOfStudyPage = new ObjectsOfStudyPageObjects(context);
            objectsOfStudyPage.SearchLoopButton.Click();
        }

        [When(@"I searched (.*) data in the objects section")]
        public void IEnteredDataInTheSearchField(string input)
        {
            var objectsOfStudyPage = new ObjectsOfStudyPageObjects(context);
            objectsOfStudyPage.SearchField.SendKeys(input);
            objectsOfStudyPage.SearchField.SendKeys(Keys.Enter);
            driver.WaitFor(7);

        }
        #endregion

        #region Then
        [Then(@"I must see object of study (.*) as first search result")]
        public void IMustSeeObjectOfStudyInTheSearchResults(string objectOfStudyTitle)
        {
            var objectOfStudyPage = new ObjectsOfStudyPageObjects(context);
            var actualTitle = objectOfStudyPage.TitleOfTheFirstObject.Text;
            Assert.Equal(objectOfStudyTitle, actualTitle);
        }

        [Then(@"I must not see object of study (.*) as first search result")]
        public void IMustNotSeeObjectOfStudyAsFirstResult(string objectOfStudyTitle)
        {
            var objectOfStudyPage = new ObjectsOfStudyPageObjects(context);
            var actualTitle = objectOfStudyPage.TitleOfTheFirstObject.Text;
            Assert.NotEqual(objectOfStudyTitle, actualTitle);
        }

        [Then(@"I must see person (.*) as first search result")]
        public void IMustSeePersonAsFirstSearchResult(string personTitle)
        {
            var objectOfStudyPage = new ObjectsOfStudyPageObjects(context);
            var actualPersonTitle = objectOfStudyPage.PersonSearchResult.Text;
            Assert.Equal(personTitle, actualPersonTitle);
        }

        [Then(@"I must see search results counter value that equal to (.*) value")]
        public void IMustSeeSearchResultsThatEqualToValue(string searchResultValueInTheSearchField)
        {
            var objectOfStudyPage = new ObjectsOfStudyPageObjects(context);
            var actualSearchResultValueInTheSearchField = objectOfStudyPage.SearchCounterInOOSSearchField.Text;
            Assert.Equal(actualSearchResultValueInTheSearchField, searchResultValueInTheSearchField);
        }

        #endregion
    }
}