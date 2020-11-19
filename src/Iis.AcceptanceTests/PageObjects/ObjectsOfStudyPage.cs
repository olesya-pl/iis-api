using Iis.AcceptanceTests.Helpers;
using OpenQA.Selenium;
using SeleniumExtras.PageObjects;
using TechTalk.SpecFlow;

namespace Iis.AcceptanceTests.PageObjects
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

        public IWebElement ObjectOfStudySmallCard;

    }
}