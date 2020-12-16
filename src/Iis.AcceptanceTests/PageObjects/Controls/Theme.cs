using AcceptanceTests.Helpers;
using OpenQA.Selenium;

namespace Iis.AcceptanceTests.PageObjects.Controls
{
    public class Theme
    {
        private IWebDriver driver;

        private IWebElement themeElement;

        public string Type =>
            //TODO: add selector for them type row
            themeElement.FindElement(By.ClassName("table_1_column_3")).Text;

        public string Name =>
            //TODO: add selector for them name row
            themeElement.FindElement(By.ClassName("el-table_1_column_1")).Text;

        public string Comment =>
            //TODO: add selector for them name row
            themeElement.FindElement(By.ClassName("el-table_1_column_2")).Text;

        public int TotalCount =>
            //TODO: add selector for Total Items count row
            int.Parse(themeElement.FindElement(By.ClassName("table_1_column_4")).Text);

        public int NewCount =>
            //TODO: add selector for New Items count row
            int.Parse(themeElement.FindElement(By.ClassName("table_1_column_5")).Text);

        public bool Displayed => themeElement.Displayed;
        public IWebElement EditButton => themeElement.FindElement(By.ClassName("el-icon-edit"));
        public IWebElement DeleteButton => themeElement.FindElement(By.ClassName("el-icon-delete"));
        public IWebElement ListViewButton => themeElement.FindElement(By.ClassName("el-icon-tickets"));
        public IWebElement MapViewButton => themeElement.FindElement(By.ClassName("el-icon-aim"));

        public void DeleteTheme()
        {
            DeleteButton.Click();
            driver.WaitFor(0.5);
            driver.FindElement(By.ClassName("el-button--primary")).Click();
            driver.WaitFor(0.2);
        }
        public Theme(IWebDriver driver, IWebElement webElement)
        {
            this.driver = driver;
            themeElement = webElement;
        }
        public Theme(IWebDriver driver, string value)
        {
            this.driver = driver;
            themeElement = driver.FindElement(By.XPath($@"//div[contains(text(),'{value}')]"));
        }
    }
}