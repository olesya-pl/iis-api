using Iis.AcceptanceTests.Helpers;
using Iis.AcceptanceTests.PageObjects;
using OpenQA.Selenium;
using TechTalk.SpecFlow;
using Xunit;


namespace Iis.AcceptanceTests.UISteps
{
    [Binding]
    public class AdministrationSteps
    {
        private readonly IWebDriver driver;
        private readonly ScenarioContext context;

        public AdministrationSteps(ScenarioContext injectedContext)
        {
            context = injectedContext;
            driver = context.GetDriver();
        }

        [When(@"I navigated to Administration page")]
        public void IWantNavigateToAdministrationPage()
        {
            var administrationPage = new AdministrationPageObjects(context);
            administrationPage.AdministrationPage.Click();

            driver.WaitFor(5);
        }

        [Then(@"I must see the Administration page")]
        public void ThenIMustSeeAdminPage()
        {
            Assert.Contains("admin/users?page=1", driver.Url);
        }

        [Then(@"I must see first user in the user list")]
        public void ThenIMustSeeFirstUserInTheUsersList()
        {
            var eventsPage = new AdministrationPageObjects(context);
            Assert.True(eventsPage.FirstUserOnTheAdminPage.Displayed);
        }
    }
}