using AcceptanceTests.Steps;
using System;
using OpenQA.Selenium;
using AcceptanceTests.Steps;
using TechTalk.SpecFlow;
using Xunit;
using TechTalk.SpecFlow.Assist.ValueRetrievers;
using OpenQA.Selenium.Support.UI;

namespace AcceptanceTests.Contour.UISteps
{
	public class AuthorizationUI : InitiateWebDriver
	{
		private readonly IWebDriver driver;

		public AuthorizationUI(ScenarioContext injectedContext): base(injectedContext)
		{
			driver = context.GetDriver();
		}

		[Given(@"I want to sign in with the user (.*) and password (.*) in the Contour")]
		public void IWantToAuthorizeInTheContour(string login, string password)
		{
			driver.WithTimeout(5).Navigate().GoToUrl(homeUrl);
			//driver.Manage().Timeouts().ImplicitWait;

			IWebElement loginField =
				driver.FindElement(By.CssSelector("div[name='username'] input"));

			loginField.SendKeys(login);

			IWebElement passwordField =
				driver.FindElement(By.CssSelector("div[name='password'] input"));

			passwordField.SendKeys(password);

			IWebElement submitButton =
				driver.FindElement(By.ClassName("login-button"));

			submitButton.Click();

			driver.WaitFor(15);
		}
	}
}