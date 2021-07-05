using AcceptanceTests.Helpers;
using AcceptanceTests.PageObjects;
using OpenQA.Selenium;
using TechTalk.SpecFlow;
using Xunit;

namespace AcceptanceTests.UISteps
{
    [Binding]
    public class AuthorizationSteps
    {
        private readonly ScenarioContext context;
        private readonly IWebDriver driver;

        private LoginPageObjects loginPageObjects;

        public AuthorizationSteps(ScenarioContext injectedContext, IWebDriver driver)
        {
            loginPageObjects = new LoginPageObjects(driver);
            context = injectedContext;
            this.driver = driver;
        }

        #region Given/When
        [Given(@"I sign in with the user (.*) and password (.*) in the Contour")]
        public void IWantToAuthorizeInTheContour(string login, string password)
        {
            loginPageObjects.Navigate();
            loginPageObjects.LoginField.SendKeys(login);
            loginPageObjects.PasswordField.SendKeys(password);
            loginPageObjects.LoginButton.Click();       
            driver.WaitFor(15);
        }

        [When(@"I pressed Sign out button")]
        public void WhenIPressedSignOutButton()
        {
            loginPageObjects.LogOutButton.Click();
        }

        [When(@"I confirmed the log out operation")]
        public void WhenITheLogOutOperation()
        {
            loginPageObjects.ConfirmLogOutButton.Click();
        }
        #endregion

        #region Then
        [Then(@"I redirected to objects page")]
        public void ThenIRedirectedToObjectsPage()
        {
            var actualLink = driver.Url;
            Assert.Contains("objects/?page=1", driver.Url);
        }

        [Then(@"Login button is active")]
        public void LoginButtonMustMeActive()
        {
            Assert.True(loginPageObjects.LoginButton.Enabled);
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
            Assert.True(loginPageObjects.ErrorMessage.Displayed);
        }

        [Then(@"I must see the Contour main page")]
        public void ThenIMustSeeTheContourMainPage()
        {
            Assert.True(loginPageObjects.LoginField.Displayed);
            Assert.True(loginPageObjects.PasswordField.Displayed);
        }
        #endregion
    }
}