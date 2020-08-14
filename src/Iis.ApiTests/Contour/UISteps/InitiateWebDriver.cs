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
		protected readonly string objectsUrl;
		protected readonly string pageTitle;

		public InitiateWebDriver(ScenarioContext injectedContext)
		{
			context = injectedContext;

			var driver = new ChromeDriver();
			//driver.Manage().Timeouts().ImplicitWait.Add(TimeSpan.FromSeconds(20));
			//driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(30);
			homeUrl = "http://qa.contour.net/";
			objectsUrl = "http://qa.contour.net/objects/?page=1";
			pageTitle = "Контур";
			driver.Manage().Window.Maximize();
			
			context.SetDriver(driver);
		}
	}
}
