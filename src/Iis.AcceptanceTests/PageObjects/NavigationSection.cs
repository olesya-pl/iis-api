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

        [FindsBy(How = How.CssSelector, Using = ".sidebar .inputStream")]
        [CacheLookup]
        public IWebElement MaterialsLink;

        [FindsBy(How = How.CssSelector, Using = ".sidebar .objects")]
        [CacheLookup]
        public IWebElement ObjectOfStudyLink;

        [FindsBy(How = How.CssSelector, Using = ".sidebar .wiki")]
        [CacheLookup]
        public IWebElement WikiLink;

        [FindsBy(How = How.CssSelector, Using = ".sidebar .admin")]
        [CacheLookup]
        public IWebElement AdministrationLink;

        [FindsBy(How = How.CssSelector, Using = ".sidebar .events")]
        [CacheLookup]
        public IWebElement EventsLink;

        [FindsBy(How = How.CssSelector, Using = ".sidebar .reports")]
        [CacheLookup]
        public IWebElement ReportsLink;

        [FindsBy(How = How.CssSelector, Using = ".sidebar .map")]
        [CacheLookup]
        public IWebElement MapLink;

        [FindsBy(How = How.CssSelector, Using = ".sidebar .themes")]
        [CacheLookup]
        public IWebElement ThemesLink;

        [FindsBy(How = How.CssSelector, Using = ".sidebar .uploaderInputStream")]
        [CacheLookup]
        public IWebElement UploaderInputStreamLink;

        [FindsBy(How = How.CssSelector, Using = ".sidebar .res")]
        [CacheLookup]
        public IWebElement ReoLink;

        [FindsBy(How = How.XPath, Using = "//*[@class='menu__group']//*[@class='el-menu-item menu__item chat']//*[text()=' Повідомлення ']")]
        [CacheLookup]
        public IWebElement ChatLink;
    }
}