using AcceptanceTests.Steps;
using OpenQA.Selenium;
using System;
using System.Threading;
using TechTalk.SpecFlow;
using Xunit;

namespace AcceptanceTests.Contour.UISteps
{
	public class AuthorizationUI : InitiateWebDriver
	{
		private readonly ScenarioContext context;

		public AuthorizationUI(ScenarioContext injectedContext)
		{
			context = injectedContext;

		}

		[Given(@"I want to sign in with the user (.*) and password (.*) in the Contour")]
		public void IWantToAuthorizeInTheContour(string login, string password)
		{
			driver.Navigate().GoToUrl(homeUrl);

			Thread.Sleep(TimeSpan.FromSeconds(5));

			IWebElement loginField =
				driver.FindElement(By.CssSelector("div[name='username'] input"));

			loginField.SendKeys(login);

			IWebElement passwordField =
				driver.FindElement(By.CssSelector("div[name='password'] input"));

			passwordField.SendKeys(password);

			IWebElement submitButton =
				driver.FindElement(By.ClassName("login-button"));

			submitButton.Click();
            Thread.Sleep(TimeSpan.FromSeconds(15));

			context.SetDriver(driver);
		}

		[Then(@"I see object page")]
		public void ThenISeeObjectPage()
		{
			Assert.Equal(objectsUrl, driver.Url);
			driver.Quit();
		}
	}
}