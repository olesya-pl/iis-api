using AcceptanceTests.Helpers;
using OpenQA.Selenium;
using SeleniumExtras.PageObjects;

namespace AcceptanceTests.PageObjects
{
    public class NavigationSection
    {
        private readonly IWebDriver driver;

        public NavigationSection(IWebDriver driver)
        {

            this.driver = driver;

            PageFactory.InitElements(driver, this);
        }

        [FindsBy(How = How.CssSelector, Using = ".sidebar__nav .inputStream")]
        [CacheLookup]
        public IWebElement MaterialsLink;

        [FindsBy(How = How.CssSelector, Using = ".sidebar__nav .objects")]
        [CacheLookup]
        public IWebElement ObjectOfStudyLink;

        [FindsBy(How = How.CssSelector, Using = ".sidebar__nav .wiki")]
        [CacheLookup]
        public IWebElement WikiLink;

        [FindsBy(How = How.CssSelector, Using = ".sidebar__nav .admin")]
        [CacheLookup]
        public IWebElement AdministrationLink;

        [FindsBy(How = How.CssSelector, Using = ".sidebar__nav .events")]
        [CacheLookup]
        public IWebElement EventsLink;

        [FindsBy(How = How.CssSelector, Using = ".sidebar__nav .reports")]
        [CacheLookup]
        public IWebElement ReportsLink;

        [FindsBy(How = How.CssSelector, Using = ".sidebar__nav .map")]
        [CacheLookup]
        public IWebElement MapLink;

        [FindsBy(How = How.CssSelector, Using = ".sidebar__nav .themes")]
        [CacheLookup]
        public IWebElement ThemesLink;

        [FindsBy(How = How.CssSelector, Using = ".sidebar__nav .uploaderInputStream")]
        [CacheLookup]
        public IWebElement UploaderInputStreamLink;
    }
}