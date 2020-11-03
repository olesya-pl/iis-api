using AcceptanceTests.Helpers;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using TechTalk.SpecFlow;

namespace AcceptanceTests.UISteps
{
	[Binding]
	public class ScrollBars
	{
		private readonly ScenarioContext context;
		private readonly IWebDriver driver;

		public ScrollBars(ScenarioContext injectedContext)
		{
			context = injectedContext;

			driver = context.GetDriver();
		}

		#region Given/When
		[Given(@"I scroll down to the element (.*)")]
		public void GivenIScrollDownToTheElement(string element)
		{
			IWebElement elementToInteract = driver.FindElement(By.CssSelector(element));
			Actions actions = new Actions(driver);
			actions.MoveToElement(elementToInteract);
			actions.Perform();
		}

		#endregion


	}
}
