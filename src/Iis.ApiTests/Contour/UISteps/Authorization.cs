using Nest;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.Threading;
using TechTalk.SpecFlow;
using Xunit;

namespace AcceptanceTests.Contour.UISteps
{
	public class AuthorizationUI : InitiateWebDriver
	{
		[Given(@"I want to sign in with the user (.*) and password (.*) in the Contour")]
		public void IWantToAuthorizeInTheContour(string login, string password)
		{
			//driver.Manage().Timeouts().ImplicitWait.Add(TimeSpan.FromSeconds(20));
			//driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(15);
			driver.Navigate().GoToUrl(homeUrl);

			//driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(15);
			//Thread.Sleep(TimeSpan.FromSeconds(5));

			//WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(30));

			WebDriverWait wait = new WebDriverWait(driver, timeout: TimeSpan.FromSeconds(30))
			{
				PollingInterval = TimeSpan.FromSeconds(15),
			};
			wait.IgnoreExceptionTypes(typeof(NoSuchElementException));

			var foo = wait.Until(drv => drv.FindElement(By.CssSelector("div[name='username'] input")));

			//IWebElement userNameField =
			//	driver.FindElement(By.CssSelector("div[name='username'] input"));

			//IWebElement userNameField =
			//	driver.FindElement(By.CssSelector("div[name='username'] input"));
			//IWebElement userNameField =
			//		wait.Until((d) => driver.FindElement(By.CssSelector("div[name='username'] input")));

			//IWebElement userNameField =
			//		wait.Until(e => driver.FindElement(By.CssSelector("div[name='username'] input")));
			//userNameField.SendKeys(login);
			foo.SendKeys(login);

			var passwordField =
				driver.FindElement(By.CssSelector("div[name='password'] input"));

			passwordField.SendKeys(password);

			var submitButton =
				driver.FindElement(By.ClassName("login-button"));

			submitButton.Click();
            Thread.Sleep(TimeSpan.FromSeconds(10));
		}

		[Then(@"I see object page")]
		public void ThenISeeObjectPage()
		{
			Assert.Equal(objectsUrl, driver.Url);
			driver.Quit();
		}
	}
}