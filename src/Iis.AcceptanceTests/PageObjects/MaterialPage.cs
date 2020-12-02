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
        public DropDown PriorityDropDown => new DropDown(driver, By.CssSelector("[_lpchecked] .el-select"));

    }
}