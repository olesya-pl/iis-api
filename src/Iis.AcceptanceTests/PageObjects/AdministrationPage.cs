using System;
using Iis.AcceptanceTests.Helpers;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.PageObjects;
using TechTalk.SpecFlow;

namespace Iis.AcceptanceTests.PageObjects
{
    public class AdministrationPageObjects
    {
        private readonly ScenarioContext context;
        private readonly IWebDriver driver;

        public AdministrationPageObjects(ScenarioContext injectedContext)
        {
            context = injectedContext;

            driver = context.GetDriver();

            PageFactory.InitElements(driver, this);
        }

        [FindsBy(How = How.CssSelector, Using = "li:nth-of-type(7) > .sidebar__nav-item-title")]
        [CacheLookup]
        public IWebElement AdministrationPage;

        [FindsBy(How = How.CssSelector, Using = "tbody > tr:nth-of-type(1)")]
        [CacheLookup]
        public IWebElement FirstUserOnTheAdminPage;

        
    }
}
