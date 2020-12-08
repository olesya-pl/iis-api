using System;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace AcceptanceTests.Helpers
{
    public static class WebDriverExtensions 
    {
        public static IWebDriver WithTimeout(this IWebDriver driver, double seconds)
        {
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(seconds);

            return driver;
        }
        public static IWebDriver WaitFor(this IWebDriver driver, double seconds)
        {
            var expectedMessage = $"Timed out after {seconds} seconds";

            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(seconds));

            try
            {
                wait.Until(d => false);
                return driver;
            }
            catch(WebDriverTimeoutException timeOutException) when (timeOutException.Message == expectedMessage)
            {
                return driver;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}