using AcceptanceTests.Helpers;
using OpenQA.Selenium;
using TechTalk.SpecFlow;

namespace AcceptanceTests.ToRefactor
{
	[Binding]
	public class KeyBoardInput
	{
		private readonly ScenarioContext context;
		private readonly IWebDriver driver;

		public KeyBoardInput(ScenarioContext injectedContext)
		{
			context = injectedContext;

			driver = context.GetDriver();
		}

		#region Given/When
		[Given(@"I press Down button plus Enter button on the (.*) item")]
		public void WhenIPressDownButtonPlusEnterButtonOnTheItem(string item)
		{
			IWebElement itemToPress = driver.FindElement(By.CssSelector(item));

			itemToPress.SendKeys(Keys.ArrowDown);

			itemToPress.SendKeys(Keys.Enter);
		}
		#endregion

	}
}
