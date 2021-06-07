using Iis.AcceptanceTests.PageObjects.Controls;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
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

        [FindsBy(How = How.CssSelector, Using = ".reports-table .p-datatable-tbody > tr")]
        public IWebElement FirstReportInTheReportList;

        [FindsBy(How = How.XPath, Using = "//span[contains(text(),'Нове зведення')]")]
        public IWebElement NewReportButton;

        [FindsBy(How = How.XPath, Using = "//button[@class='el-button add-button el-button--default']")]
        public IWebElement CreateNewReportButton;

        [FindsBy(How = How.CssSelector, Using = ".left-table .el-button--icon")]
        public IWebElement AddEventToReportButton;

        [FindsBy(How = How.CssSelector, Using = "div[name='title'] .el-input__inner")]
        public IWebElement NameOfTheReportField;

        [FindsBy(How = How.CssSelector, Using = "th.reports-table__created-date.p-sortable-column")]
        public IWebElement HourAndDateColumn;

        public Report GetReportByTitle(string title)
        {
            return new Report(driver, title);
        }

        public bool IsElementVisible()
        {
            try
            {
                var elem = driver.FindElement(By.CssSelector(".events-table tbody.p-datatable-tbody > tr td.simple-events-table__name-cell"));
                return elem.Displayed;
            }
            catch
            {
                return false;
            }

        }

        public ReportCreationAndEdit ReportCreationAndEdit => new ReportCreationAndEdit(driver);
    }
}