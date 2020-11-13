using Iis.AcceptanceTests.Helpers;
using OpenQA.Selenium;
using SeleniumExtras.PageObjects;
using TechTalk.SpecFlow;

namespace Iis.AcceptanceTests.PageObjects
{
    public class MaterialsPageObjects
    {
        private readonly ScenarioContext context;

        private readonly IWebDriver driver;

        public MaterialsPageObjects(ScenarioContext injectedContext)
        {
            context = injectedContext;

            driver = context.GetDriver();

            PageFactory.InitElements(driver, this);
        }

        [FindsBy(How = How.CssSelector, Using = "div:nth-of-type(1) > li:nth-of-type(8)")]
        [CacheLookup]
        public IWebElement MaterialsSection;

        [FindsBy(How = How.CssSelector, Using = "tbody > tr:nth-of-type(1)")]
        [CacheLookup]
        public IWebElement FirstMaterialInTheMaterialsList;

        [FindsBy(How = How.CssSelector, Using = ".el-button--default")]
        [CacheLookup]
        public IWebElement SearchButton;

        [FindsBy(How = How.CssSelector, Using = "span > .el-input > .el-input__inner")]
        [CacheLookup]
        public IWebElement SearchField;

        [FindsBy(How = How.CssSelector, Using = ".is-scrolling-none .el-table__empty-block")]
        [CacheLookup]
        public IWebElement EmptySearchField;

        [FindsBy(How = How.CssSelector, Using = ".el-button--success")]
        [CacheLookup]
        public IWebElement ProcessedButton;

        [FindsBy(How = How.CssSelector, Using = "div:nth-of-type(1) > .meta-data-card > .meta-data-expand  .el-button.el-button--default > span")]
        [CacheLookup]
        public IWebElement ShowMLResultsButton;

        [FindsBy(How = How.CssSelector, Using = "ul[role='menubar'] > li:nth-of-type(5)")]
        [CacheLookup]
        public IWebElement EventsTab;

        [FindsBy(How = How.CssSelector, Using = ".el-menu > .el-menu-item:nth-of-type(4)")]
        [CacheLookup]
        public IWebElement ObjectsTab;

        [FindsBy(How = How.CssSelector, Using = ".el-menu > .el-menu-item:nth-of-type(2)")]
        [CacheLookup]
        public IWebElement MLTab;

        [FindsBy(How = How.CssSelector, Using = ".el-header.material-relations__header > .el-select.material-relations-input")]
        [CacheLookup]
        public IWebElement EventsTabSearch;

        [FindsBy(How = How.CssSelector, Using = ".el-menu > .el-menu-item:nth-of-type(2)")]
        [CacheLookup]
        public IWebElement MLTabSearch;

        [FindsBy(How = How.CssSelector, Using = ".material-objects__header .material-relations-input")]
        [CacheLookup]
        public IWebElement ObjectsTabSearch;

        [FindsBy(How = How.CssSelector, Using = "div:nth-of-type(1) > .general-container > div:nth-of-type(1) > .el-form-item__content > .el-select.el-tooltip")]
        [CacheLookup]
        public IWebElement ImportanceDropDown;

        [FindsBy(How = How.CssSelector, Using = "div:nth-of-type(2) > .el-form-item__content > .el-select.el-tooltip")]
        [CacheLookup]
        public IWebElement AuthenticityDropDown;

        [FindsBy(How = How.CssSelector, Using = "div:nth-of-type(3) > .el-form-item__content > .el-select.el-tooltip  .el-input__inner")]
        [CacheLookup]
        public IWebElement RelevanceDropDown;

        [FindsBy(How = How.CssSelector, Using = "div:nth-of-type(4) > .el-form-item__content > .el-select.el-tooltip")]
        [CacheLookup]
        public IWebElement СompletenessOfInformation;

        [FindsBy(How = How.CssSelector, Using = "div:nth-of-type(2) > .general-container > .el-form-item > .el-form-item__content > .el-select.el-tooltip")]
        [CacheLookup]
        public IWebElement SourceCredibility;

        [FindsBy(How = How.CssSelector, Using = "div:nth-of-type(3) > .general-container > .el-form-item > .el-form-item__content > .el-select.el-tooltip")]
        [CacheLookup]
        public IWebElement Originator;
    }
}