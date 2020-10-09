using AcceptanceTests.Steps;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Text;
using TechTalk.SpecFlow;

namespace AcceptanceTests.Contour.UISteps
{
	[Binding]
	public class TerminateWebDriver
	{
		private readonly ScenarioContext context;

		public TerminateWebDriver(ScenarioContext injectedContext)
		{
			context = injectedContext;

		}

		[AfterScenario]
		public void TerminateWebDriverAfterTests()
		{
			IWebDriver driver = context.GetDriver();

			driver.Quit();
		}
	}
}
