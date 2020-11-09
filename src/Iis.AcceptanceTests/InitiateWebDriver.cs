using System;
using Iis.AcceptanceTests.Helpers;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Remote;
using TechTalk.SpecFlow;

namespace Iis.AcceptanceTests.UISteps
{
	
	public class InitiateWebDriver
	{
		protected readonly ScenarioContext context;
		protected readonly string homeUrl;
		protected readonly string pageTitle;

		public InitiateWebDriver(ScenarioContext injectedContext)
		{
			var driverOptions = new ChromeOptions();

			var runName = GetType().Assembly.GetName().Name;
			var timestamp = $"{DateTime.Now:yyyyMMdd.HHmm}";

			driverOptions.AddAdditionalCapability("name", runName, true);
			driverOptions.AddAdditionalCapability("enableVNC", true, true);
			driverOptions.AddAdditionalCapability("screenResolution", "1920x1080x24", true);

			context = injectedContext;
			//var driver = new ChromeDriver();
			var driver = new RemoteWebDriver(new Uri("http://localhost:4444/wd/hub"), driverOptions);
			//homeUrl = "http://qa.contour.net/";
			pageTitle = "Контур";
			driver.Manage().Window.Maximize();
			
			context.SetDriver(driver);
		}
	}
}
