using AcceptanceTests.Helpers;
using OpenQA.Selenium;
using TechTalk.SpecFlow;

namespace AcceptanceTests.UISteps
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
