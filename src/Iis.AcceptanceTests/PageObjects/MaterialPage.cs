using AcceptanceTests.PageObjects.Controls;
using OpenQA.Selenium;

namespace AcceptanceTests.PageObjects
{
    public class MaterialPage
    {
        private readonly IWebDriver driver;

        public MaterialPage(IWebDriver driver)
        {
            this.driver = driver;
        }
        public DropDown ImportanceDropDown => new DropDown(driver, By.CssSelector("div:nth-of-type(1) > .general-container > div:nth-of-type(1) > .el-form-item__content > .el-select.el-tooltip"));
        public DropDown ReliabilityDropDown => new DropDown(driver, By.XPath("//div[@class='el-select el-tooltip action-select--reliability']"));
        public DropDown SessionPriorityDropDown => new DropDown(driver, By.CssSelector(".action-select--session-priority"));
        public DropDown SourceCredibilityDropDown => new DropDown(driver, By.XPath("//div[@class='el-select el-tooltip action-select--sourcereliability']"));
    }
}