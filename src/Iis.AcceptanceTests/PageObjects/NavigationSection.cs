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

        [FindsBy(How = How.CssSelector, Using = ".sidebar__nav li.inputStream")]
        [CacheLookup]
        public IWebElement MaterialsLink;

        [FindsBy(How = How.CssSelector, Using = ".sidebar__nav li.objects")]
        [CacheLookup]
        public IWebElement ObjectOfStudyLink;

        [FindsBy(How = How.CssSelector, Using = ".sidebar__nav li.wiki")]
        [CacheLookup]
        public IWebElement WikiLink;

        [FindsBy(How = How.CssSelector, Using = ".sidebar__nav li.admin")]
        [CacheLookup]
        public IWebElement AdministrationLink;

        [FindsBy(How = How.CssSelector, Using = ".sidebar__nav li.events")]
        [CacheLookup]
        public IWebElement EventsLink;

        [FindsBy(How = How.CssSelector, Using = ".sidebar__nav li.reports")]
        [CacheLookup]
        public IWebElement ReportsLink;

        [FindsBy(How = How.CssSelector, Using = ".sidebar__nav li.map")]
        [CacheLookup]
        public IWebElement MapLink;

        [FindsBy(How = How.CssSelector, Using = ".sidebar__nav li.themes")]
        [CacheLookup]
        public IWebElement ThemesLink;

        [FindsBy(How = How.CssSelector, Using = ".sidebar__nav li.uploaderInputStream")]
        [CacheLookup]
        public IWebElement UploaderInputStreamLink;
    }
}