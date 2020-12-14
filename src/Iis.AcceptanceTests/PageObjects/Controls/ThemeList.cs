using OpenQA.Selenium;

namespace Iis.AcceptanceTests.PageObjects.Controls
{
    public class Theme
    {
        private IWebDriver driver;

        private IWebElement themeElement;

        public bool Displayed => themeElement.Displayed;

        public Theme(IWebDriver driver, string value)
        {
            this.driver = driver;
            themeElement = driver.FindElement(By.XPath($@"//div[contains(text(),'{value}')]"));
        }
    }
}