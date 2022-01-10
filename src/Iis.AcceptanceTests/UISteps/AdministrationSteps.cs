using AcceptanceTests.Helpers;
using AcceptanceTests.PageObjects;
using OpenQA.Selenium;
using TechTalk.SpecFlow;
using Xunit;

namespace AcceptanceTests.UISteps
{
    [Binding]
    public class AdministrationSteps
    {
        private readonly IWebDriver driver;
		private readonly ScenarioContext context;
        private AdministrationPageObjects administrationPage;
        private readonly NavigationSection navigationSection;

        public IWebElement VersionInfo =>
            driver.FindElement(By.CssSelector(".el-notification .el-notification__group .el-notification__title"));

        public AdministrationSteps(ScenarioContext injectedContext, IWebDriver driver)
        {
            administrationPage = new AdministrationPageObjects(driver);
            navigationSection = new NavigationSection(driver);
            context = injectedContext;
            this.driver = driver;
        }

        [When(@"I navigated to Administration page")]
        public void IWantNavigateToAdministrationPage()
        {
            navigationSection.AdministrationLink.Click();
            driver.WaitFor(5);
        }

        [When(@"I checked of version by product")]
        public void WhenICheckedOfVersionByProduct()
        {
            navigationSection.ObjectOfStudyLink.SendKeys(Keys.Alt + Keys.Shift + "V");
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
            Assert.True(administrationPage.FirstUserOnTheAdminPage.Displayed);
        }

        [Then(@"I must see version by product")]
        public void ThenIMustSeeVersionByProduct()
        {
            Assert.True(VersionInfo.Displayed);
        }
    }
}