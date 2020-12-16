using System;
using System.Collections.Generic;
using System.Text;
using OpenQA.Selenium;

namespace AcceptanceTests.PageObjects
{
    public class ThemeSideView
    {
        private readonly IWebDriver driver;
        private IWebElement sideViewElement;

        public ThemeSideView(IWebDriver driver)
        {
            this.driver = driver;
            sideViewElement = driver.FindElement(By.ClassName("divide-body__aside"));
        }

        public bool Displayed => sideViewElement.Displayed;
    }
}
