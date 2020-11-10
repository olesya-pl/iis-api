using Iis.AcceptanceTests.Helpers;
using Iis.AcceptanceTests.PageObjects;
using OpenQA.Selenium;
using TechTalk.SpecFlow;
using Xunit;


namespace Iis.AcceptanceTests.UISteps
{
    [Binding]
    public class EventsSteps
    {
        private readonly IWebDriver driver;
        private readonly ScenarioContext context;

        public EventsSteps(ScenarioContext injectedContext)
        {
            context = injectedContext;
            driver = context.GetDriver();
        }

        [When(@"I navigated to Events page")]
        public void IWantNavigateToEventsPage()
        {
            var eventsPage = new EventsPageObjects(context);
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
            var eventsPage = new EventsPageObjects(context);
            Assert.True(eventsPage.FirstEventInTheEventsList.Displayed);
        }
    }
}