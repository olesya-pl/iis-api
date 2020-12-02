using System;
using Iis.AcceptanceTests.Helpers;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.PageObjects;

namespace AcceptanceTests.PageObjects
{
    public class LoginPageObjects
    {
        private IWebDriver driver;
        private WebDriverWait wait;
        private string LoginPageUrl = TestData.BaseAddress + "/login";

        public LoginPageObjects(IWebDriver driver)
        {
            this.driver = driver;
            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            PageFactory.InitElements(driver, this);
        }

        public void Navigate()
        {
            driver.WithTimeout(10).Navigate().GoToUrl(LoginPageUrl);
        }


        [FindsBy(How = How.ClassName, Using = "login-button")]
        [CacheLookup]
        public IWebElement LoginButton;

        [FindsBy(How = How.CssSelector, Using = "div[name='username'] input")]
        [CacheLookup]
        public IWebElement LoginField;

        [FindsBy(How = How.CssSelector, Using = "div[name='password'] input")]
        [CacheLookup]
        public IWebElement PasswordField;

        [FindsBy(How = How.ClassName, Using = "error-message")]
        [CacheLookup]
        public IWebElement ErrorMessage;

    }
}
