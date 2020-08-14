using System;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace AcceptanceTests.Steps
{
    public static class WebDriverExtensions 
    {
        public static IWebDriver WithTimeout(this IWebDriver driver, double seconds)
        {
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(seconds);

            return driver;
        }
        public static void WaitFor(this IWebDriver driver, double seconds)
        {
            var expectedMessage = $"Timed out after {seconds} seconds";

            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(seconds));

            try
            {
                wait.Until(d => false);
            }
            catch(WebDriverTimeoutException timeOutException) when (timeOutException.Message == expectedMessage)
            {
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}