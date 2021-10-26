using OpenQA.Selenium;

namespace Iis.AcceptanceTests.PageObjects.Controls
{
    public class Event
    {
        private IWebDriver driver;
        private IWebElement eventElement;


        public Event(IWebDriver driver)
        {
            this.driver = driver;
        }

        public string Name =>
        //TODO: add selector for an event name row
        eventElement.FindElement(By.CssSelector("td.events-table__name > div.events-table__name-cell")).Text;

        public void Click()
        {
            eventElement.Click();
        }

        public Event(IWebDriver driver, IWebElement webElement)
        {
            this.driver = driver;
            eventElement = webElement;
        }

        public Event(IWebDriver driver, string value)
        {
            this.driver = driver;
            eventElement = driver.FindElement(By.XPath($@"//main/section/main/form/div[5]/div/div/span"));
        }


    }
}