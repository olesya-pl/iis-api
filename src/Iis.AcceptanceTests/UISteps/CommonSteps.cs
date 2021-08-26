using System;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using TechTalk.SpecFlow;

namespace AcceptanceTests.UISteps
{
    [Binding]
    public class CommonSteps
    {
        private readonly ScenarioContext _context;
        private readonly IWebDriver _driver;

        public CommonSteps(ScenarioContext injectedContext, IWebDriver driver)
        {
            _context = injectedContext;
            _driver = driver;
        }

        [When(@"Loading icon is not displayed")]
        public void LoadingIconIsNotDisplayed()
        {
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(180));
            wait.Until(driver => !_driver.FindElement(By.CssSelector(".el-loading-mask")).Displayed);
        }


    }
}
