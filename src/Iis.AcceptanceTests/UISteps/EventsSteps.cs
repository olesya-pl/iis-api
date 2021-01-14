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
        #endregion
    }
}

