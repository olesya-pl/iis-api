using AcceptanceTests.Helpers;
using OpenQA.Selenium;

namespace AcceptanceTests.PageObjects.Controls
{
    public class OOSHierarchy
    {
        private readonly IWebElement hierarchy;
        private readonly IWebDriver driver;

        public OOSHierarchy(IWebDriver driver, By by)
        {
            this.driver = driver;
            hierarchy.FindElement(by);
        }

        public void Find(string value)
        {
            driver.WaitFor(1);
            hierarchy.FindElement(By.XPath($@"//div[contains(text(),""{value}"")]"));
        }
    }
}