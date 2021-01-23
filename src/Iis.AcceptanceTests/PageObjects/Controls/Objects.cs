using AcceptanceTests.Helpers;
using OpenQA.Selenium;

namespace Iis.AcceptanceTests.PageObjects.Controls
{
    public class ObjectsSearch
    {
        private IWebDriver driver;

        private IWebElement objectsElement;

        public bool Displayed => objectsElement.Displayed;

        public ObjectsSearch(IWebDriver driver, string value)
        {
            this.driver = driver;
            driver.WaitFor(3);
            driver.FindElement(By.XPath($@"//div[contains(text(),'{value}')]"));
        }
    }
}