using OpenQA.Selenium;

namespace Iis.AcceptanceTests.PageObjects.Controls
{
    public class ReportCreationAndEdit
    {
        private readonly IWebDriver driver;
        private IWebElement reportCreationAndEditElement;

        public bool Displayed => reportCreationAndEditElement.Displayed;

        public ReportCreationAndEdit(IWebDriver driver)
        {
            this.driver = driver;
            reportCreationAndEditElement = driver.FindElement(By.ClassName("el-form"));
        }

        public IWebElement EnterRecipientField => reportCreationAndEditElement.FindElement(By.XPath("//div[@name='recipient']//input[@type='text']"));
        public IWebElement ProceedButton => reportCreationAndEditElement.FindElement(By.XPath("//span[contains(text(),'Продовжити')]"));
        public IWebElement FirstEventCheckboxInTheEventsList => reportCreationAndEditElement.FindElement(By.XPath("//table[@class='el-table__body']/tbody/tr[1]/td[2]//span[@class='el-checkbox__inner']"));
        public IWebElement AddEventToReport => reportCreationAndEditElement.FindElement(By.ClassName("el-button bluegrey-icon el-button--icon"));
        public IWebElement FirstEventCheckboxInTheReportsList => reportCreationAndEditElement.FindElement(By.CssSelector(".right-table .el-table__body-wrapper .el-checkbox__inner"));
        public IWebElement EventInTheReportList => reportCreationAndEditElement.FindElement(By.CssSelector(".right-table .el-table__row"));
        public IWebElement RemoveEventFromReport => reportCreationAndEditElement.FindElement(By.ClassName("el-button flip-left bluegrey-icon el-button--icon"));
        public IWebElement SaveButton => reportCreationAndEditElement.FindElement(By.XPath("//span[contains(text(),' Зберегти ')]"));
        public IWebElement ConfirmButton => reportCreationAndEditElement.FindElement(By.XPath("//span[contains(text(),'Підтвердити')]"));
    }
}