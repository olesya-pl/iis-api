using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;

namespace Iis.AcceptanceTests.PageObjects.Controls
{
    public class Report
    {
        private IWebDriver driver;
        private IWebElement reportElement;

        public bool Displayed => reportElement.Displayed;

        public Report(IWebDriver driver, IWebElement webElement)
        {
            this.driver = driver;
            reportElement = webElement;

        }

        public Report(IWebDriver driver, string value)
        {
            this.driver = driver;
            reportElement = driver.FindElement(By.XPath($@"//div[contains(text(),'{value}')]"));
        }

        public void MouseHoverOnReport()
        {
            Actions action = new Actions(driver);
            action.MoveToElement(reportElement).Perform();
        }
    }
}