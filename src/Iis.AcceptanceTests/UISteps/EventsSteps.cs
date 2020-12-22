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
            driver.WaitFor(10);
        }

        [When(@"I clicked on the (.*) event in the event list")]
        public void WhenIGotAllEventsList(string eventName)
        {
            //var xyz = eventsPage.Events.First().Click();
            // var events = eventsPage.GetEventsByName("захід");
            // var eventTitle = eventsPage.GetEventByTitle("Захід");
        }

        [When(@"I clicked on the (.*) event in the event list")]
        public void WhenIClickedOnTheEventInTheEventList(string value)
        {
            var activeEvent = eventsPage.GetEventByTitle(value);
            //activeEvent.Click();
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

