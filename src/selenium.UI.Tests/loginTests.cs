using System;
using Xunit;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace selenium.UI.Tests
{
    public class loginTests
    {
        [Fact]
        [Trait("Category", "Smoke")]
        public void LoadApplicationPage() 
        {
            using (IWebDriver driver = new ChromeDriver()) {
                driver.Navigate().GoToUrl("http://dev.contour.net/login");
            }
        }
    }
}
