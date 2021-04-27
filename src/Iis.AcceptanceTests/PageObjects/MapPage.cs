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

        [FindsBy(How = How.XPath, Using = "//div[@class='sidebar__body']//li[starts-with(@class, 'el-menu-item sidebar__nav-item')]//div[contains(text(),'Мапа')]")]
        [CacheLookup]
        public IWebElement MapSection;

        [FindsBy(How = How.XPath, Using = "//form[@role='search']")]
        [CacheLookup]
        public IWebElement MapSearch;
    }
}