using AcceptanceTests.Steps;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Text;
using TechTalk.SpecFlow;

namespace AcceptanceTests.Contour.UISteps
{
	
	public class InitiateWebDriver
	{
		protected readonly ScenarioContext context;
		protected readonly string homeUrl;
		protected readonly string pageTitle;

		public InitiateWebDriver(ScenarioContext injectedContext)
		{
			context = injectedContext;

			var driver = new ChromeDriver();
			homeUrl = "http://qa.contour.net/";
			pageTitle = "Контур";
			driver.Manage().Window.Maximize();
			
			context.SetDriver(driver);
		}
	}
}
