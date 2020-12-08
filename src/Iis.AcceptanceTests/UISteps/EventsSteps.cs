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

        public EventsSteps(ScenarioContext injectedContext, IWebDriver driver)
        {
            context = injectedContext;
            this.driver = driver;
        }

        [When(@"I navigated to Events page")]
        public void IWantNavigateToEventsPage()
        {
            var eventsPage = new EventsPageObjects(driver);
            eventsPage.EventsPage.Click();

            driver.WaitFor(10);
        }

        [Then(@"I must see the Events page")]
        public void ThenIMustSeeEventCreationButton()
        {
            Assert.Contains("events/?page=1", driver.Url);
        }

        [Then(@"I must see first event in the events list")]
        public void ThenIMustSeeFirstEventInTheEventsList()
        {
            var eventsPage = new EventsPageObjects(driver);
            Assert.True(eventsPage.FirstEventInTheEventsList.Displayed);
        }
    }
}