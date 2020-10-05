using AcceptanceTests.Steps;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Text;
using TechTalk.SpecFlow;
using Xunit;

namespace AcceptanceTests.Contour.UISteps
{
	[Binding]
	public class Windows
	{
		private readonly ScenarioContext context;
		private readonly IWebDriver driver;

		public Windows(ScenarioContext injectedContext)
		{
			context = injectedContext;

			driver = context.GetDriver();
		}

		[Then(@"I must see the (.*) window")]
		public void ThenIMustSeeTheWindow(string window)
		{
			IWebElement windowElement = driver.FindElement(By.CssSelector(window));
			Assert.True(windowElement.Displayed);
		}

	}
}
