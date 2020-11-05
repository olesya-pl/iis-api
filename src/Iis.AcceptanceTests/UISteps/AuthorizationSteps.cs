using Iis.AcceptanceTests.Helpers;
using Iis.AcceptanceTests.PageObjects;
using OpenQA.Selenium;
using TechTalk.SpecFlow;
using Xunit;

namespace Iis.AcceptanceTests.UISteps
{
    [Binding]
    public class AuthorizationSteps : InitiateWebDriver
    {
        private readonly IWebDriver driver;

        public AuthorizationSteps(ScenarioContext injectedContext) : base(injectedContext)
        {
            driver = context.GetDriver();
        }

        [Given(@"I sign in with the user (.*) and password (.*) in the Contour")]
        public void IWantToAuthorizeInTheContour(string login, string password)
        {
            var loginPage = new LoginPage(driver);
            loginPage.Navigate();
            loginPage.LoginField.SendKeys(login);
            loginPage.PasswordField.SendKeys(password);
            loginPage.LoginButton.Click();
            
            driver.WaitFor(15);
        }

        [Then(@"I redirected to objects page")]
        public void ThenIRedirectedToObjectsPage()
        {
            Assert.Contains("objects/?page=1", driver.Url);
        }

        [Then(@"Login button is active")]
        public void LoginButtonMustMeActive()
        {
            var loginPage = new LoginPage(driver);
            Assert.True(loginPage.LoginButton.Enabled);
        }

        [Then(@"Login and password inputs are highlighted with red")]
        public void ThenTheFieldAndFieldMustBeHighlightedWithRedColor()
        {
            var redBorderedInputs = driver.FindElements(By.ClassName("has-error"));
            Assert.True(redBorderedInputs.Count == 2);
        }

        [Then(@"I see the error message that login or password is incorrect")]
        public void ThenISeeTheLoginErrorMessage()
        {
            var loginPage = new LoginPage(driver);
            Assert.True(loginPage.ErrorMessage.Displayed);
        }
    }
}