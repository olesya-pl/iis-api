using System;
using System.Collections.Generic;
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

        public static IWebElement FindElementByAnyXpath(this IWebDriver driver, IEnumerable<string> xPathCollection)
        {
            foreach (var selector in xPathCollection)
            {
                try
                {
                    var element = driver.FindElement(By.XPath(selector));
                    return element;
                }
                catch (NoSuchElementException e)
                {
                    continue;
                }
            }
            throw new NoSuchElementException();
        }


        public static bool HasClass(this IWebElement element, string className)
        {
            return element.GetAttribute("class").Contains(className);
        }
    }
}