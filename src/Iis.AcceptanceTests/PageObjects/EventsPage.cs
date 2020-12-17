using System.Collections.Generic;
using System.Linq;
using Iis.AcceptanceTests.PageObjects.Controls;
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

        [FindsBy(How = How.XPath, Using = "//li[@name='events']")]
        public IWebElement EventsPage;

        [FindsBy(How = How.CssSelector, Using = ".add-button")]
        [CacheLookup]
        public IWebElement CreateEventButton;

        [FindsBy(How = How.CssSelector, Using = "tbody > tr:nth-of-type(1)")]
        [CacheLookup]
        public IWebElement FirstEventInTheEventsList;

        public List<Event> Events => driver.FindElements(By.ClassName("el-table__row"))
                    .Select(webElement => new Event(driver, webElement)).ToList();

        public List<Event> GetEventsByName(string eventName)
        {
            return Events.Where(i => i.Name.Contains(eventName)).ToList();
        }
        public Event GetEventByTitle(string title)
        {
            return new Event(driver, title);
        }
    }
}
