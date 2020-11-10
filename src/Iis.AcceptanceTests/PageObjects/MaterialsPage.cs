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
    }
}