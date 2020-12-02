using OpenQA.Selenium;
using SeleniumExtras.PageObjects;

namespace AcceptanceTests.PageObjects
{
    public class ReportPageObjects
    {
        public IWebDriver driver;

        public ReportPageObjects(IWebDriver driver)
        {
            this.driver = driver;
            PageFactory.InitElements(driver, this);
        }

        [FindsBy(How = How.CssSelector, Using = "div:nth-of-type(1) > li:nth-of-type(3)")]
        [CacheLookup]
        public IWebElement ReportSection;

        [FindsBy(How = How.CssSelector, Using = "tbody .el-table__row:nth-of-type(1)")]
        [CacheLookup]
        public IWebElement FirstReportInTheReportList;
    }
}