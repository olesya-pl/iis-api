using AcceptanceTests.Steps;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Remote;
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
			var driverOptions = new ChromeOptions();
			context = injectedContext;	
			//var driver = new ChromeDriver();
			var driver = new RemoteWebDriver(new Uri("http://localhost:4444/wd/hub"), driverOptions);
			homeUrl = "http://qa.contour.net/";
			pageTitle = "Контур";
			driver.Manage().Window.Maximize();
			
			context.SetDriver(driver);
		}
	}
}
