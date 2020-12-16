using AcceptanceTests.Helpers;
using OpenQA.Selenium;

namespace AcceptanceTests.PageObjects.Controls
{
    public class DropDown
    {
        private IWebElement dropDown;
        private IWebElement inputValue;
        private IWebDriver driver;

        public string Text => inputValue.GetAttribute("value");

        public DropDown(IWebDriver driver, By by)
        {
            this.driver = driver;
            dropDown = driver.FindElement(by);
            inputValue = dropDown.FindElement(By.TagName("input"));
        }

        public void Select(string value)
        {
            dropDown.Click();
            driver.WaitFor(0.5);
            driver.FindElement(By.XPath($@"//li//span[text()[contains(.,""{value}"")]]")).Click();
        }
    }
}