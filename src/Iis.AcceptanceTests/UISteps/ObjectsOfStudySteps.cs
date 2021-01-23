using System;
using System.Linq;
using AcceptanceTests.Helpers;
using AcceptanceTests.PageObjects;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using TechTalk.SpecFlow;
using Xunit;

namespace AcceptanceTests.UISteps
{
    [Binding]
    public class ObjectsOfStudySteps
    {
        private readonly IWebDriver driver;
        private readonly ScenarioContext context;
        private ObjectsOfStudyPageObjects objectsOfStudyPage;
        public ObjectsOfStudySteps(ScenarioContext injectedContext, IWebDriver driver)
        {
            objectsOfStudyPage = new ObjectsOfStudyPageObjects(driver);
            context = injectedContext;
            this.driver = driver;
        }

        #region When

        [When(@"I clicked on search button in the Object of study section")]
        public void IClickedOnSearchButton()
        {
            objectsOfStudyPage.SearchLoopButton.Click();
        }

        [When("I clicked on enlarge small card button")]
        public void WhenIClickedOnEnlargeSmallCardButton()
        {
            objectsOfStudyPage.EnlargeObjectOfStudySmallCardButton.Click();
        }

        [When(@"I got search counter value in the Object of study section")]
        public void IGetSearchCounterValueInTheObjectOfStudySection()
        {
            context.SetResponse("counterValue", objectsOfStudyPage.OOSSearchCounter.Text);
        }

        [When(@"I clicked on first object of study")]
        public void IClickedOnFirstObjectOfStudy()
        {
            objectsOfStudyPage.TitleOfTheFirstObject.Click();
        }

        [When(@"I searched (.*) data in the Objects of study section")]
        public void IEnteredDataInTheSearchField(string input)
        {
            objectsOfStudyPage.SearchField.SendKeys(input);
            objectsOfStudyPage.SearchField.SendKeys(Keys.Enter);
            driver.WaitFor(7);

        }

        [When(@"I clicked on the first search result title in the Objects of study section")]
        public void WhenIClickedOnTheFirstSearchResultTitle()
        {
            objectsOfStudyPage.FirstSearchResultTitle.Click();
        }

        [When(@"I clicked on the Edit button in the Objects of study section")]
        public void WhenIClickedOnTheEditButtonInTheObjectsOfStudySection()
        {
            objectsOfStudyPage.EditObjectOfStudyButton.Click();
        }

        [When(@"I clicked on the Classifier block in the big card window")]
        public void WhenIClickedOnTheClassifierBlockInTheBigCardWindow()
        {
            objectsOfStudyPage.ClassifierBlock.Click();
        }

        [When(@"I clicked on the General info block in the big card window")]
        public void WhenIClickedOnTheGeneralInfoBlockInTheBigCardWindow()
        {
            objectsOfStudyPage.GeneralInfoBlock.Click();
        }

        [When(@"I clicked on the Direct reporting relationship link in the big card window")]
        public void WhenIClickedOnTheDirectReportingRelationshipLinkInTheBigCardWindow()
        {
            objectsOfStudyPage.ClassifierBlock.Click();
        }

        [When(@"I clicked on the relations tab")]
        public void WhenIClickedOnTheRelationTab()
        {
            objectsOfStudyPage.RelationsTab.Click();
        }

        [When(@"I clicked on Hierarchy tab in the Object of study section")]
        public void WhenIClickedOnHierarchyTabInTheObjectOfStudySectiion()
        {
            objectsOfStudyPage.HierarchyTab.Click();
        }

        [When(@"I double clicked on the (.*) card in the hierarchy")]
        public void WhenIDoubleClickedOnTheExpandButton(string cardName)
        {
            driver.WaitFor(1);
            objectsOfStudyPage.GetHierarchyCardByTitle(cardName).DoubleClickOnCard();
        }

        [When(@"I double clicked on the (.*) expand button in the hierarchy")]
        public void WhenIDoubleClickedOnTheExpandButtonInTheHierarchy(string cardName)
        {
            objectsOfStudyPage.GetHierarchyCardByTitle(cardName).Expand();
        }

        [When(@"I clicked on the create a new object of study button")]
        public void WhenIClickOnTheCreateANewObjectOfStudyButton()
        {
            objectsOfStudyPage.CreateANewObjectOfStudyButton.Click();
            driver.WaitFor(2);
        }

        [When(@"I clicked on the create a new military organization button")]
        public void WhenIClickOnTheCreateANewMilitaryOrganizationButton()
        {
            objectsOfStudyPage.CreateAMilitaryOrganizationButton.Click();
        }

        [When(@"I entered the (.*) value in the affiliation field")]
        public void WhenIEnteredTheValueInTheAffiliationField(string value)
        {
            objectsOfStudyPage.AffiliationField.SendKeys(value);
            objectsOfStudyPage.AffiliationField.SendKeys(Keys.Down);
            objectsOfStudyPage.AffiliationField.SendKeys(Keys.Enter);
        }

        [When(@"I entered the (.*) value in the importance field")]
        public void WhenIEnteredTheValueInTheImportanceField(string value)
        {
            objectsOfStudyPage.ImportanceField.SendKeys(value);
            objectsOfStudyPage.ImportanceField.SendKeys(Keys.Down);
            objectsOfStudyPage.ImportanceField.SendKeys(Keys.Enter);
        }

        [When(@"I entered the (.*) value in the name real full field")]
        public void WhenIEnteredTheValueInTheRealNameFullField(string objectOfStudyName)
        {
            var objectOfStudyUniqueName = $"{objectOfStudyName} {DateTime.Now.ToLocalTime()} {Guid.NewGuid():N}";
            context.SetResponse("objectOfStudyName", objectOfStudyUniqueName);
            objectsOfStudyPage.RealNameFullField.SendKeys(objectOfStudyUniqueName);
            objectsOfStudyPage.RealNameFullField.SendKeys(Keys.Enter);
        }

        [When(@"I clicked on the save button to create a new object of study")]
        public void WhenIClickedOnTheSaveButtonToCreateANewObjectOfStudy()
        {
            objectsOfStudyPage.SaveObjectOfStudyButton.Click();
            driver.WaitFor(1);
        }

        [When(@"I clicked on the confirm save button to create a new object of study")]
        public void WhenIClickedOnTheConfirmSaveButtonToCreateANewObjectOfStudy()
        {
            objectsOfStudyPage.ConfirmSaveOfANewObjectOfStudyButton.Click();
            driver.WaitFor(2);
        }

        [When(@"I clicked on the classifiers block")]
        public void WhenIClickedOnTheClassifiersBlock()
        {
            objectsOfStudyPage.ClassifierBlock.Click();
            driver.WaitFor(1);
        }

        [When(@"I clicked on the dislocation block")]
        public void WhenIClickedOnTheDislocationBlock()
        {
            objectsOfStudyPage.DislocationBlock.Click();
            driver.WaitFor(2);
        }

        [When(@"I clicked on the temporary dislocation block")]
        public void WhenIClickedOnTheTemporaryDislocationBlock()
        {
            objectsOfStudyPage.TemporaryDislocationBlock.Click();
            driver.WaitFor(2);
        }

        [When(@"I entered the (.*) value in the latitude field at the dislocation block")]
        public void WhenIEnteredTheValueInTheLatitudeField(string value)
        {
            objectsOfStudyPage.LatitudeField.SendKeys(value);
        }

        [When(@"I entered the (.*) value in the country field at the dislocation block")]
        public void WhenIEnteredTheValueInTheCountryField(string value)
        {
            objectsOfStudyPage.CountryFieldInTheDisclocationBlock.SendKeys(value);
            driver.WaitFor(2);
            objectsOfStudyPage.CountryFieldInTheDisclocationBlock.SendKeys(Keys.Down);
            objectsOfStudyPage.CountryFieldInTheDisclocationBlock.SendKeys(Keys.Enter);
        }

        [When(@"I entered the (.*) value in the country field at the temporary dislocation block")]
        public void WhenIEnteredTheValueInTheCountryFieldTempDis(string value)
        {
            objectsOfStudyPage.CountryFieldInTheTemporaryDisclocationBlock.SendKeys(value);
            driver.WaitFor(2);
            objectsOfStudyPage.CountryFieldInTheTemporaryDisclocationBlock.SendKeys(Keys.Down);
            objectsOfStudyPage.CountryFieldInTheTemporaryDisclocationBlock.SendKeys(Keys.Enter);
        }


        [When(@"I entered the (.*) value in the longitude field at dislocation block")]
        public void WhenIEnteredTheValueInTheLongitudeFieldAtTheDislocationBlock(string value)
        {
            objectsOfStudyPage.LongitudeField.SendKeys(value);
        }

        [When(@"I entered the (.*) value in the longitude field at temporary dislocation block")]
        public void WhenIEnteredTheValueInTheLongitudeFieldAtTheTemporaryDislocationBlock(string value)
        {
            objectsOfStudyPage.TemporaryDislocationLongitudeField.SendKeys(value);
        }

        [When(@"I entered the (.*) value in the latitude field at the temporary dislocation block")]
        public void WhenIEnteredTheValueInTheLatitudeFieldAtTheTemporaryDislocationBlock(string value)
        {
            objectsOfStudyPage.TemporaryDislocationLatitudeField.SendKeys(value);
        }

        [When(@"I clicked on the general info block")]
        public void WhenIClickedOnTheGeneralBlock()
        {
            objectsOfStudyPage.GeneralInfoBlock.Click();
            driver.WaitFor(2);
        }

        [When(@"I entered the (.*) value in the direct reporting relationship field")]
        public void WhenIEnteredTheValueInTheDirectReportingRelationShipField(string value)
        {
            objectsOfStudyPage.DirectReportingRelationship.SendKeys(value);
            driver.WaitFor(8);
            objectsOfStudyPage.DirectReportingRelationship.SendKeys(Keys.Down);
            objectsOfStudyPage.DirectReportingRelationship.SendKeys(Keys.Enter);
        }

        [When(@"I scrolled down to the (.*) element")]
        public void WhenIScrolledDownToTheElement(string element)
        {
            objectsOfStudyPage.ScrollDown(element);
        }

        [When(@"I navigated to the Events tab in the big object of study card")]
        public void WhenINavigatedToTheEventTabInTheBigObjectOfStudyCard()
        {
            objectsOfStudyPage.EventsTabInTheBigObjectOfStudyCard.Click();
        }
        #endregion

        #region Then
        [Then(@"I must see object of study (.*) as first search result")]
        public void IMustSeeObjectOfStudyInTheSearchResults(string objectOfStudyTitle)
        {
            var actualTitle = objectsOfStudyPage.TitleOfTheFirstObject.Text;
            Assert.Equal(objectOfStudyTitle, actualTitle);
        }

        [Then(@"I must not see object of study (.*) as first search result")]
        public void IMustNotSeeObjectOfStudyAsFirstResult(string objectOfStudyTitle)
        {
            var actualTitle = objectsOfStudyPage.TitleOfTheFirstObject.Text;
            Assert.NotEqual(objectOfStudyTitle, actualTitle);
        }

        [Then(@"I must see person (.*) as first search result")]
        public void IMustSeePersonAsFirstSearchResult(string personTitle)
        {
            var actualPersonTitle = objectsOfStudyPage.PersonSearchResult.Text;
            Assert.Equal(personTitle, actualPersonTitle);
        }

        [Then(@"I must see search results counter value that equal to (.*) value")]
        public void IMustSeeSearchResultsThatEqualToValue(string searchResultValueInTheSearchField)
        {
            var actualSearchResultValueInTheSearchField = objectsOfStudyPage.SearchCounterInOOSSearchField.Text;
            Assert.Equal(actualSearchResultValueInTheSearchField, searchResultValueInTheSearchField);
        }

        [Then(@"I must see the object of study small card")]
        public void IMustSeeTheObjectOfStudySmallCard()
        {
            Assert.True(objectsOfStudyPage.ObjectOfStudySmallCardWindow.Displayed);
        }

        [Then(@"I must see the title (.*) in the small card")]
        public void IMustSeeTheTitleInTheSmallCard(string expectedTitle)
        {
            var actualTitle = objectsOfStudyPage.ObjectTitleInTheSmallCard.Text;
            Assert.Equal(expectedTitle, actualTitle);
        }

        [Then(@"I must see these tabs in the big object of study card")]
        public void IMustSeeTheseTabsInTheBigObjectOfStudyCard(Table table)
        {
            foreach (TableRow row in table.Rows)
            {
                var tableValue = row.Values.First();

                Assert.True((typeof(ObjectsOfStudyPageObjects).GetField(tableValue).GetValue(objectsOfStudyPage) as IWebElement).Displayed);
            }
        }

        [Then(@"I must see the specific text blocks in big object of study card")]
        public void ThenIMustSeeTheSpecificTextBlocksInBigObjectOfStudyCard(Table table)
        {
            foreach (TableRow row in table.Rows)
            {
                var tableValue = row.Values.First();

                Assert.True((typeof(ObjectsOfStudyPageObjects).GetField(tableValue).GetValue(objectsOfStudyPage) as IWebElement).Displayed);
            }
        }

        [Then(@"I must see that search counter values are equal in the Objects of study section")]
        public void IMustSeeThatSearchCounterValuesAreEqualInTheObjectOfStudySection()
        {
            var actualValue = objectsOfStudyPage.OOSSearchCounter.Text;
            var expectedValue = context.GetResponse<string>("counterValue");
            Assert.Equal(actualValue, expectedValue);
        }

        [Then(@"I must see zero search results in the Object of study page")]
        public void ThenIMustSeeZeroResults()
        {
            Assert.True(objectsOfStudyPage.OOSEmptySearchResults.Displayed);
        }

        [Then(@"I must see (.*) title of the object")]
        public void ThenIMustSeeObjectBigCard(string expectedObjectTitle)
        {
            driver.WaitFor(2);
            var actualObjectTitle = objectsOfStudyPage.OOSTitle.Text;
            Assert.Contains(expectedObjectTitle, actualObjectTitle);
        }

        [Then(@"I must see these cards in hierarchy")]
        public void ThenIMustNotSeeTheseCardInHierarchy(Table table)
        {
            foreach (TableRow row in table.Rows)
            {
                var tableValue = row.Values.First();
                Assert.True(objectsOfStudyPage.GetHierarchyCardByTitle(tableValue).Displayed);
            }
        }

        [Then(@"I must see the (.*) as one of the search results")]
        public void ThenIMustSeeThirdBrigadeBerkutAsOneOfTheSearchResults(string value)
        {
            //Assert.True(objectsOfStudyPage.ThirdBrigadeSearchResult.Displayed);
            Assert.True(objectsOfStudyPage.GetObjectByTitle(value).Displayed);
        }

        [Then(@"I must see the specified result")]
        public void ThenIMustSeeThirdBrigadeBerkutAsOneOfTheSearchResultss(Table table)
        {
            foreach (TableRow row in table.Rows)
            {
                var tableValue = row.Values.First();
                Assert.True(objectsOfStudyPage.GetObjectByTitle(tableValue).Displayed);
            }
        }

        [Then(@"I must see name real full is equal to the (.*) value")]
        public void ThenIMustSeeNameRealFullIsEqualToTheValue(string expectedValue)
        {
            var actualValue = objectsOfStudyPage.ThirdBrigadeSearchResult.Text;
            Assert.Equal(expectedValue, actualValue);
        }

        [Then(@"I must see that the (.*) event related to the object of study")]
        public void WhenIMustSeeAsRelatedToTheObjectOfStudyEvent(string eventName)
        {
            var eventUniqueName = context.Get<string>(eventName);
            Assert.Contains(objectsOfStudyPage.Events, _ => _.Name == eventUniqueName);
        }
        #endregion
    }
}
