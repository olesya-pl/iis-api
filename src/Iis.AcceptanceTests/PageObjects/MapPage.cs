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

        [FindsBy(How = How.XPath, Using = "//div[@class='sidebar__body']//li[@class='el-menu-item sidebar__nav-item map']")]
        public IWebElement MapSection;

        [FindsBy(How = How.XPath, Using = "//form[@role='search']")]
        public IWebElement MapSearch;
    }
}