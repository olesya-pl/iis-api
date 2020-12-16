using Iis.AcceptanceTests.PageObjects.Controls;
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

        [FindsBy(How = How.XPath, Using = "//span[contains(text(),'Нове зведення')]")]
        public IWebElement NewReportButton;

        [FindsBy(How = How.XPath, Using = "//button[@class='el-button add-button el-button--default']")]
        public IWebElement CreateNewReportButton;

        public Report GetReportByTitle(string title)
        {
            return new Report(driver, title);
        }
        public ReportCreationAndEdit ReportCreationAndEdit => new ReportCreationAndEdit(driver);
    }
}