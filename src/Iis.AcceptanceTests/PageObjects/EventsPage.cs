using OpenQA.Selenium;
using SeleniumExtras.PageObjects;

namespace AcceptanceTests.PageObjects
{

    public class EventsPageObjects
    {
        private readonly IWebDriver driver;

        public EventsPageObjects(IWebDriver driver)
        {

            this.driver = driver;

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
