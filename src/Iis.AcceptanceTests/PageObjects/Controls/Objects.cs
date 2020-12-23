using AcceptanceTests.Helpers;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;

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

        public void ScrollDown(IWebDriver driver, string value)
        {
            this.driver = driver;
            var elementToScroll = driver.FindElement(By.XPath($"//div[contains(text(),'{value}')]"));
            Actions actions = new Actions(driver);
            actions.MoveToElement(elementToScroll).Perform();
        }
    }
}