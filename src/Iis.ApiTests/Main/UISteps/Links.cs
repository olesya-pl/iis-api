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
	public class Links
	{
		private readonly ScenarioContext context;
		private readonly IWebDriver driver;

		public Links(ScenarioContext injectedContext)
		{
			context = injectedContext;

			driver = context.GetDriver();
		}

		[Then(@"I see the (.*) link in the browser navigation bar")]
		public void LinkInTheBrowserVerificaiton(string link)
		{
			Assert.Equal(link, driver.Url);
		}
	}
}
