using Iis.AcceptanceTests.Helpers;
using OpenQA.Selenium;
using SeleniumExtras.PageObjects;
using TechTalk.SpecFlow;

namespace Iis.AcceptanceTests.PageObjects
{

    public class EventsPageObjects
    {
        private readonly ScenarioContext context;
        private readonly IWebDriver driver;

        public EventsPageObjects(ScenarioContext injectedContext)
        {
            context = injectedContext;

            driver = context.GetDriver();

            PageFactory.InitElements(driver, this);
        }

        [FindsBy(How = How.CssSelector, Using = "div:nth-of-type(1) > li:nth-of-type(2)")]
        [CacheLookup]
        public IWebElement EventsPage;

        [FindsBy(How = How.CssSelector, Using = ".add-button")]
        [CacheLookup]
        public IWebElement CreateEventButton;

        [FindsBy(How = How.CssSelector, Using = "tbody > tr:nth-of-type(1)")]
        [CacheLookup]
        public IWebElement FirstEventInTheEventsList;

    }
}
