using OpenQA.Selenium;
using SeleniumExtras.PageObjects;

namespace AcceptanceTests.PageObjects
{
    public class ReoPageObjects
    {
        private IWebDriver driver;

        public ReoPageObjects(IWebDriver driver)
        {
            this.driver = driver;
            PageFactory.InitElements(driver, this);
        }

        [FindsBy(How = How.XPath, Using = "//form[@role='search']")]
        public IWebElement ReoSearch;
    }
}