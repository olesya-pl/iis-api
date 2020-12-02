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

        [FindsBy(How = How.CssSelector, Using = "li:nth-of-type(5) > .sidebar__nav-item-title")]
        [CacheLookup]
        public IWebElement MapSection;

        [FindsBy(How = How.CssSelector, Using = "[role] input.esri-search__input[type]")]
        [CacheLookup]
        public IWebElement MapSearch;
    }
}