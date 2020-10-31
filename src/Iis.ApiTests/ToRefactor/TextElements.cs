using AcceptanceTests.Helpers;
using OpenQA.Selenium;
using TechTalk.SpecFlow;
using Xunit;

namespace AcceptanceTests.UISteps
{
	[Binding]
	public class TextElements
	{
		private readonly ScenarioContext context;
		private readonly IWebDriver driver;

		public TextElements(ScenarioContext injectedContext)
		{
			context = injectedContext;

			driver = context.GetDriver();
		}

		

		[Then(@"I must see the (.*) element")]
		public void ThenIMustSeeTheElement(string element)
		{
			IWebElement buttonElement = driver.FindElement(By.CssSelector(element));
			Assert.True(buttonElement.Displayed);
		}

	    [Then(@"I must see the (.*) element in (.*) seconds")]
		public void ThenIMustSeeTheElementInSeconds(string element, int seconds)
		{
			driver.WaitFor(seconds);
			IWebElement elementToCheck = driver.FindElement(By.CssSelector(element));
			Assert.True(elementToCheck.Displayed);
		}
	}
}
