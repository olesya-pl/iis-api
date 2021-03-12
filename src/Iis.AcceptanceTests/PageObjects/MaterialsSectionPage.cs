using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using SeleniumExtras.PageObjects;
using System.Collections.Generic;
using System.Linq;

namespace AcceptanceTests.PageObjects
{
    public class MaterialsSectionPage
    {
        private readonly IWebDriver driver;

        public MaterialsSectionPage(IWebDriver driver)
        {

            this.driver = driver;

            PageFactory.InitElements(driver, this);
        }

        [FindsBy(How = How.CssSelector, Using = "div:nth-of-type(1) > li:nth-of-type(8)")]
        [CacheLookup]
        public IWebElement MaterialsSection;

        [FindsBy(How = How.XPath, Using = "//tr[@class='el-table__row']/td[1]")]
        public IWebElement FirstMaterialInTheMaterialsList;

        [FindsBy(How = How.CssSelector, Using = ".el-button--default")]
        public IWebElement SearchButton;

        [FindsBy(How = How.CssSelector, Using = ".el-input.entity-search__input > .el-input__inner")]
        public IWebElement SearchField;

        [FindsBy(How = How.CssSelector, Using = ".is-scrolling-none .el-table__empty-block")]
        [CacheLookup]
        public IWebElement EmptySearchField;

        [FindsBy(How = How.CssSelector, Using = ".el-button--success")]
        public IWebElement ProcessedButton;

        [FindsBy(How = How.CssSelector, Using = "div:nth-of-type(1) > .meta-data-card > .meta-data-expand  .el-button.el-button--default > span")]
        [CacheLookup]
        public IWebElement ShowMLResultsButton;

        [FindsBy(How = How.CssSelector, Using = ".action-tab--connection")]
        public IWebElement RelationsTab;

        [FindsBy(How = How.CssSelector, Using = ".action-tab--features")]
        public IWebElement PatternTab;

        [FindsBy(How = How.XPath, Using = "//span[contains(text(),'PhoneNumber')]/following-sibling::span[1]")]
        public IWebElement PhoneNumberPatternNode;

        [FindsBy(How = How.XPath, Using = "//div/ul/li[@class='el-menu-item action-tab--objects']")]
        public IWebElement ObjectsTab;

        [FindsBy(How = How.CssSelector, Using = ".el-menu > .el-menu-item:nth-of-type(2)")]
        [CacheLookup]
        public IWebElement MLTab;

        [FindsBy(How = How.CssSelector, Using = ".material-events__header .el-input__inner")]
        [CacheLookup]
        public IWebElement EventsSearch;

        [FindsBy(How = How.CssSelector, Using = ".el-menu > .el-menu-item:nth-of-type(2)")]
        [CacheLookup]
        public IWebElement MLTabSearch;

        [FindsBy(How = How.CssSelector, Using = "[aria-describedby] .el-input__inner")]
        public IWebElement ObjectsTabSearch;

        public MaterialPage MaterialPage => new MaterialPage(driver);

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

        [FindsBy(How = How.CssSelector, Using = ".entity-search__result-counter")]
        public IWebElement MaterialsSearchResultCounter;

        [FindsBy(How = How.CssSelector, Using = ".action-button--prev-page span")]
        public IWebElement PreviousMaterialButton;

		[FindsBy(How = How.CssSelector, Using = ".table-zones__body .el-table  .el-table__body  .el-table__row")]
		public IWebElement FirstSearchResult;

        [FindsBy(How = How.CssSelector, Using = ".meta-data__list .meta-data__list-item:nth-of-type(3) .el-button--default")]
        [CacheLookup]
        public IWebElement TextClassifierMLOutputButton;

        [FindsBy(How = How.CssSelector, Using = ".meta-data__list .meta-data__list-item:nth-of-type(3) .meta-data-card__result-body")]
        [CacheLookup]
        public IWebElement TextClassifierMLOutputForm;

        [FindsBy(How = How.CssSelector, Using = ".is-scrolling-none")]
        [CacheLookup]
        public IWebElement EmptyAreInTheMaterialList;

        [FindsBy(How = How.CssSelector, Using = ".material-objects .material-objects-table a")]
        public IWebElement ConnectedObjectLink;

        [FindsBy(How = How.CssSelector, Using = ".confirm-message-box__action-confirm")]
        public IWebElement ConfirmDeleteRelationButton;

        [FindsBy(How = How.CssSelector, Using = ".cell > div:nth-of-type(2)")]
        [CacheLookup]
        public IWebElement FirstSearchResultContentBlock;

        [FindsBy(How = How.XPath, Using = "//button[@name='delete']")]
        [CacheLookup]
        public IWebElement DeleteRelatedObjectOfStudy;

        [FindsBy(How = How.XPath, Using = "//button[@name='delete']")]
        public IWebElement DeleteRelatedEventButton;

        public void ScrollToEnd()
        {
            Actions actions = new Actions(driver);
            actions.SendKeys(Keys.Control).SendKeys(Keys.End).Perform();
        }

        public List<MaterialRelatedItems> MaterialsRelatedEvents => driver.FindElements(By.CssSelector(".material-events .material-events-table .el-table__row"))
                   .Select(_ => new MaterialRelatedItems(driver, _)).ToList();

        public List<MaterialRelatedItems> MaterialsRelatedObjects => driver.FindElements(By.CssSelector(".material-objects .material-objects-table .el-table__row"))
                  .Select(_ => new MaterialRelatedItems(driver, _)).ToList();

        public MaterialRelatedItems GetItemTitleRelatedToMaterial(string title)
        {
            return new MaterialRelatedItems(driver, title);
        }
    }
}