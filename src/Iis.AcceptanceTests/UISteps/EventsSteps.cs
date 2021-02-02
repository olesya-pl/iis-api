using System;
using System.Linq;
using AcceptanceTests.Helpers;
using AcceptanceTests.PageObjects;
using OpenQA.Selenium;
using TechTalk.SpecFlow;
using Xunit;

namespace AcceptanceTests.UISteps
{
    [Binding]
    public class EventsSteps
    {
        private readonly IWebDriver driver;
        private readonly ScenarioContext context;

        private EventsPageObjects eventsPage;

        #region Given/When
        public EventsSteps(ScenarioContext injectedContext, IWebDriver driver)
        {
            eventsPage = new EventsPageObjects(driver);
            context = injectedContext;
            this.driver = driver;
        }

        [When(@"I navigated to Events page")]
        public void IWantNavigateToEventsPage()
        {
            eventsPage.EventsPage.Click();
            driver.WaitFor(3);
        }

        [When(@"I created a new (.*) event")]
        public void WhenICreatedANewEvent(string eventName)
        {
            var eventUniqueName = $"{eventName} {DateTime.Now.ToLocalTime()} {Guid.NewGuid().ToString("N")}";
            context.Set(eventUniqueName, eventName);
            eventsPage.CreateEventButton.Click();
            eventsPage.EventTitle.SendKeys(eventUniqueName);
            eventsPage.AverageImportaceRadioButton.Click();
            eventsPage.FlawRadioButton.Click();
            eventsPage.CountrySelectionDropDown.Click();
            var countryInput = eventsPage.CountrySelectionDropDown.FindElement(By.TagName("input"));
            countryInput.SendKeys("Росія");
            driver.WaitFor(2);
            countryInput.SendKeys(Keys.Down);
            countryInput.SendKeys(Keys.Enter);
            eventsPage.EventTypeDropDown.Click();
            driver.WaitFor(2);
            eventsPage.EventTypeDropDown.SendKeys(Keys.Up);
            eventsPage.EventTypeDropDown.SendKeys(Keys.Enter);
            eventsPage.EventComponentDropDown.SendKeys("Кризові регіони");
            eventsPage.EventComponentDropDown.SendKeys(Keys.Down);
            eventsPage.EventComponentDropDown.SendKeys(Keys.Enter);
            eventsPage.SaveEventChangesButton.Click();
            driver.WaitFor(2);
            eventsPage.ConfirmSaveEventChangesButton.Click();
            driver.WaitFor(2);
            driver.Navigate().Refresh();
            eventsPage.CloseEventCreationWindow.Click();
        }

        [When(@"I created a new (.*) event by filling in all the data")]
        public void WhenICreatedANewТестоваПодіяEventByFillingInAllTheData(string eventName)
        {

            var eventUniqueName = $"{eventName} {DateTime.Now.ToLocalTime()} {Guid.NewGuid().ToString("N")}";
            context.Set(eventUniqueName, eventName);
            eventsPage.CreateEventButton.Click();
            eventsPage.EventTitle.SendKeys(eventUniqueName);
            eventsPage.DescriptionField.SendKeys("Додаткові тестові дані");
            eventsPage.AverageImportaceRadioButton.Click();
            eventsPage.FlawRadioButton.Click();
            eventsPage.CountrySelectionDropDown.Click();
            var countryInput = eventsPage.CountrySelectionDropDown.FindElement(By.TagName("input"));
            countryInput.SendKeys("Росія");
            driver.WaitFor(2);
            countryInput.SendKeys(Keys.Down);
            countryInput.SendKeys(Keys.Enter);
            eventsPage.EventTypeDropDown.Click();
            driver.WaitFor(2);
            eventsPage.EventTypeDropDown.SendKeys(Keys.Up);
            eventsPage.EventTypeDropDown.SendKeys(Keys.Enter);
            eventsPage.EventComponentDropDown.SendKeys("Кризові регіони");
            eventsPage.EventComponentDropDown.SendKeys(Keys.Down);
            eventsPage.EventComponentDropDown.SendKeys(Keys.Enter);
            eventsPage.SaveEventChangesButton.Click();
            driver.WaitFor(2);
            eventsPage.ConfirmSaveEventChangesButton.Click();
            driver.WaitFor(2);
            driver.Navigate().Refresh();
            eventsPage.CloseEventCreationWindow.Click();
        }


        [When(@"I searched for the (.*) created event")]
        public void WhenISearchedForTheCreatedEvent(string eventName)
        {
            eventsPage.SearchButton.Click();
            var eventUniqueName = context.Get<string>(eventName);
            eventsPage.SearchField.SendKeys($"\"{eventUniqueName}\"");
            eventsPage.SearchField.SendKeys(Keys.Enter);
        }

        [When(@"I pressed the edit event button")]
        public void WhenIPressedTheEditEventButton()
        {
            eventsPage.EditButton.Click();
            driver.WaitFor(3);
        }

        [When(@"I binded a (.*) material to the event")]
        public void WhenIBindedAMaterialToTheEvent(string materialName)
        {
            var materialInput = eventsPage.BindedMaterialsField.FindElement(By.TagName("input"));
            materialInput.SendKeys(materialName);
            driver.WaitFor(2);
            materialInput.SendKeys(Keys.Down);
            materialInput.SendKeys(Keys.Enter);
        }

        [When(@"I binded an (.*) object of study to the event")]
        public void WhenIBindedAnObjectOfStudyToTheEvent(string objectOfStudy)
        {
            var objectOfStudyInput = eventsPage.BindedObjectsOfStudyField.FindElement(By.TagName("input"));
            objectOfStudyInput.SendKeys(objectOfStudy);
            driver.WaitFor(4);
            objectOfStudyInput.SendKeys(Keys.Down);
            objectOfStudyInput.SendKeys(Keys.Enter);
            driver.WaitFor(2);
        }

        [When(@"I pressed the save event changes button")]
        public void WhenIPressedTheSaveButton()
        {
            eventsPage.SaveEventChangesButton.Click();
            driver.WaitFor(2);
        }

        [When(@"I pressed the confirm save changes in the event")]
        public void WhenIPressedTheConfirmSaveChangesInTheEvent()
        {
            eventsPage.ConfirmSaveEventChangesButton.Click();
            driver.WaitFor(2);
        }

        [When(@"I reloaded the event page")]
        public void WhenIReloadedTheEventPage()
        {
            driver.Navigate().Refresh();
            driver.WaitFor(5);
        }

        [When(@"I clicked on the (.*) binded object of study in the event")]
        public void WhenIClickedOnTheBindedObjectOfStudyInTheEvent(string objectOfStudy)
        {
            eventsPage.GetRelatedObjectOfStudyNameBindedToTheEvent(objectOfStudy).Click();
        }
        #endregion

        #region Then
        [Then(@"I must see the Events page")]
        public void ThenIMustSeeEventCreationButton()
        {
            Assert.Contains("events/?page=1", driver.Url);
        }

        [Then(@"I must see first event in the events list")]
        public void ThenIMustSeeFirstEventInTheEventsList()
        {
            Assert.True(eventsPage.FirstEventInTheEventsList.Displayed);
        }

        [Then(@"I must see the (.*) material binded to the event")]
        public void ThenIMustSeeTheMaterialBindedToTheEvent(string materialName)
        {
            Assert.True(eventsPage.IsMaterialVisible(materialName));
        }

        [Then(@"I must see the (.*) event in the event search results")]
        public void ThenIMustSeeTheEvent(string eventName)
        {
            var eventUniqueName = context.Get<string>(eventName);
            Assert.True(eventsPage.IsEventVisible(eventUniqueName));
        }
        #endregion
    }
}