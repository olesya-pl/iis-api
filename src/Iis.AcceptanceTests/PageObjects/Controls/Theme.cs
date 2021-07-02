using AcceptanceTests.Helpers;
using OpenQA.Selenium;

namespace AcceptanceTests.PageObjects.Controls
{
    public class Theme
    {
        private readonly IWebDriver _driver;

        private readonly IWebElement _themeTableElement;

        public string Title =>
            _themeTableElement.FindElement(By.CssSelector(".theme .theme__title .cell")).Text;

        public string Comment =>
            _themeTableElement.FindElement(By.CssSelector(".theme .theme__comment .cell")).Text;

        public string TypeName =>
            _themeTableElement.FindElement(By.CssSelector(".theme .theme__type-name .cell")).Text;

        public int TotalCount =>
            int.Parse(_themeTableElement.FindElement(By.CssSelector(".theme .theme__total-count .cell")).Text);

        public int UnwatchCount =>
            int.Parse(_themeTableElement.FindElement(By.CssSelector(".theme .theme__unwatch-count .cell")).Text);

        public bool Displayed => _themeTableElement.Displayed;
        public IWebElement EditButton => _themeTableElement.FindElement(By.CssSelector(".theme__controls .theme__action--edit"));
        public IWebElement DeleteButton => _themeTableElement.FindElement(By.XPath("//button[@class='el-button el-tooltip theme__action--delete el-button--default']"));
        public IWebElement ListViewButton => _themeTableElement.FindElement(By.CssSelector(".theme__controls .theme__action--open-list-view"));
        public IWebElement MapViewButton => _themeTableElement.FindElement(By.CssSelector(".theme__controls .theme__action--open-map-view"));

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
            _themeTableElement = driver.FindElement(By.XPath($@"//div[contains(text(),'{value}')]"));
        }
    }
}