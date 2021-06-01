using AcceptanceTests.Helpers;
using OpenQA.Selenium;

namespace AcceptanceTests.PageObjects.Controls
{
    public class Theme
    {
        private readonly IWebDriver _driver;

        private readonly IWebElement _themeTableElement;

        public string Title =>
            _themeTableElement.FindElement(By.CssSelector(".themes-table .themes-table__title")).Text;

        public string Comment =>
            _themeTableElement.FindElement(By.CssSelector(".themes-table .themes-table__comment")).Text;

        public string TypeName =>
            _themeTableElement.FindElement(By.CssSelector(".themes-table .themes-table__type-title")).Text;

        public int TotalCount =>
            int.Parse(_themeTableElement.FindElement(By.CssSelector(".themes-table .themes-table__total-count")).Text);

        public int UnwatchCount =>
            int.Parse(_themeTableElement.FindElement(By.CssSelector(".themes-table .themes-table__unwatch-count")).Text);

        public bool Displayed => _themeTableElement.Displayed;
        public IWebElement EditButton => _themeTableElement.FindElement(By.CssSelector(".themes-table .themes-table__controls .theme__action--edit"));
        public IWebElement DeleteButton => _themeTableElement.FindElement(By.XPath("//button[@class='el-button el-tooltip theme__action--delete el-button--default']"));
        public IWebElement ListViewButton => _themeTableElement.FindElement(By.CssSelector(".themes-table .themes-table__controls .theme__action--open-list-view"));
        public IWebElement MapViewButton => _themeTableElement.FindElement(By.CssSelector(".themes-table .themes-table__controls .theme__action--open-map-view"));

        public void DeleteTheme()
        {
            DeleteButton.Click();
            _driver.WithTimeout(0.5).FindElement(By.CssSelector(".el-button--primary")).Click();
            _driver.WaitFor(0.2);
        }
        public Theme(IWebDriver driver, IWebElement webElement)
        {
            _driver = driver;
            _themeTableElement = webElement;
        }

        public override string ToString()
        {
            return Title;
        }
        public Theme(IWebDriver driver, string value)
        {
            _driver = driver;
            _themeTableElement = driver.FindElement(By.XPath($@"//tbody[@class='p-datatable-tbody']/tr/td[contains(text(), '{value}')]/ancestor::tr"));
        }
    }
}