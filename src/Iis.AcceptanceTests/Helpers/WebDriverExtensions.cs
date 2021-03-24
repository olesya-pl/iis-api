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

        public static IWebElement FindElementByAnyXpath(this IWebElement driver, IEnumerable<string> xPathCollection)
        {
            //var oldTime = driver.Manage().Timeouts().ImplicitWait;
            //driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromMilliseconds(1);

            foreach (var selector in xPathCollection)
            {
                try
                {
                    var element = driver.FindElement(By.XPath(selector));
                    //driver.Manage().Timeouts().ImplicitWait = oldTime;
                    return element;
                }
				catch (NoSuchElementException)
				{
                    continue;
                }
            }

            //driver.Manage().Timeouts().ImplicitWait = oldTime;
            throw new NoSuchElementException();
        }


        public static bool HasClass(this IWebElement element, string className)
        {
            return element.GetAttribute("class").Contains(className);
        }
    }
}