using OpenQA.Selenium;
using SeleniumExtras.PageObjects;

namespace AcceptanceTests.PageObjects
{
    public class MapPageObjects
    {
        private IWebDriver driver;

        public MapPageObjects(IWebDriver driver)
        {
            this.driver = driver;
            PageFactory.InitElements(driver, this);
        }

        [FindsBy(How = How.CssSelector, Using = ".sidebar__nav li.map")]
        public IWebElement MapSection;

        [FindsBy(How = How.XPath, Using = "//form[@role='search']")]
        public IWebElement MapSearch;
    }
}