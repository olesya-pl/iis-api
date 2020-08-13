using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Text;

namespace AcceptanceTests.Contour.UISteps
{
	public class InitiateWebDriver
	{
		protected readonly ChromeDriver driver;
		protected readonly string homeUrl;
		protected readonly string objectsUrl;
		protected readonly string pageTitle;

		public InitiateWebDriver()
		{
			driver = new ChromeDriver();
			//driver.Manage().Timeouts().ImplicitWait.Add(TimeSpan.FromSeconds(20));
			//driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(30);
			homeUrl = "http://qa.contour.net/";
			driver.Manage().Window.Maximize();			
		}
	}
}
