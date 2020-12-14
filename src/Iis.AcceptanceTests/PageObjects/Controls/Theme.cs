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
            themeElement.FindElement(By.ClassName("")).Text;

        public string Name =>
            //TODO: add selector for them name row
            themeElement.FindElement(By.ClassName("")).Text;

        public int TotalCount
        {
            get
            {
                //TODO: add selector for Total Items count row
                int.Parse(themeElement.FindElement(By.ClassName("")).Text);
                return 1;
            }
        }
        public int NewCount
        {
            get
            {
                //TODO: add selector for New Items count row
                int.Parse(themeElement.FindElement(By.ClassName("")).Text);
                return 1;
            }
        }

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