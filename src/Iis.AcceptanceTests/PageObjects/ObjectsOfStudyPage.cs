using OpenQA.Selenium;
using SeleniumExtras.PageObjects;

namespace AcceptanceTests.PageObjects
{
    public class ObjectsOfStudyPageObjects
    {
        public IWebDriver driver;

        public ObjectsOfStudyPageObjects(IWebDriver driver)
        {
            this.driver = driver;
            PageFactory.InitElements(driver, this);
        }


        [FindsBy(How = How.CssSelector, Using = ".el-button.el-button--default.el-tooltip")]
        [CacheLookup]
        public IWebElement SearchLoopButton;

        [FindsBy(How = How.CssSelector, Using = "span > .el-input > .el-input__inner")]
        [CacheLookup]
        public IWebElement SearchField;

        [FindsBy(How = How.CssSelector, Using = ".el-table__row:nth-of-type(1) .text-ellipsis.title")]
        [CacheLookup]
        public IWebElement TitleOfTheFirstObject;

        [FindsBy(How = How.CssSelector, Using = ".summary-person-row .title")]
        [CacheLookup]
        public IWebElement PersonSearchResult;

        [FindsBy(How = How.CssSelector, Using = ".entity-search__result-counter")]
        [CacheLookup]
        public IWebElement SearchCounterInOOSSearchField;

        [FindsBy(How = How.CssSelector, Using = "tbody > tr:nth-of-type(1)")]
        [CacheLookup]
        public IWebElement FirstElementInTheOOSList;

        [FindsBy(How = How.CssSelector, Using = "tbody > tr:nth-of-type(1)")]
        [CacheLookup]
        public IWebElement ObjectOfStudySmallCardWindow;

        [FindsBy(How = How.CssSelector, Using = ".text-ellipsis span")]
        [CacheLookup]
        public IWebElement ObjectTitleInTheSmallCard;

        [FindsBy(How = How.CssSelector, Using = "button[name='btn-full-screen']")]
        [CacheLookup]
        public IWebElement EnlargeObjectOfStudySmallCardButton;

        [FindsBy(How = How.CssSelector, Using = ".el-menu > [role='menuitem']:nth-of-type(1)")]
        [CacheLookup]
        public IWebElement BigCardProfileTab;

        [FindsBy(How = How.CssSelector, Using = ".el-menu > .el-menu-item:nth-of-type(2)")]
        [CacheLookup]
        public IWebElement BigCardMaterialsTab;

        [FindsBy(How = How.CssSelector, Using = ".el-menu > .el-menu-item:nth-of-type(3)")]
        [CacheLookup]
        public IWebElement BigCardEventsTab;

        [FindsBy(How = How.CssSelector, Using = "ul[role='menubar'] > li:nth-of-type(4)")]
        [CacheLookup]
        public IWebElement BigCardChangeHistoryTab;

        [FindsBy(How = How.CssSelector, Using = ".el-menu > .el-menu-item:nth-of-type(5)")]
        [CacheLookup]
        public IWebElement BigCardMapTab;

        [FindsBy(How = How.CssSelector, Using = "ul[role='menubar'] > li:nth-of-type(6)")]
        [CacheLookup]
        public IWebElement BigCardRelationsTab;

        [FindsBy(How = How.CssSelector, Using = "div[name='affiliation'] div[name='view-item-relation']")]
        [CacheLookup]
        public IWebElement BigCardAffiliation;

        [FindsBy(How = How.CssSelector, Using = "div[name='importance'] div[name='view-item-relation']")]
        [CacheLookup]
        public IWebElement BigCardImportance;

        [FindsBy(How = How.CssSelector, Using = ".entity-search__result-counter")]
        [CacheLookup]
        public IWebElement OOSSearchCounter;

        [FindsBy(How = How.CssSelector, Using = ".is-scrolling-none .el-table__empty-block")]
        [CacheLookup]
        public IWebElement OOSEmptySearchResults;

        [FindsBy(How = How.CssSelector, Using = ".el-header.theme.theme-grey > .title")]
        [CacheLookup]
        public IWebElement OOSTitle;
    }
}