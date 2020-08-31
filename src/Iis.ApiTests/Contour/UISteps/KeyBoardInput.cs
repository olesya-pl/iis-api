using AcceptanceTests.Steps;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Text;
using TechTalk.SpecFlow;

namespace AcceptanceTests.Contour.UISteps
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

		[Given(@"I press Down button plus Enter button on the (.*) item")]
		public void WhenIPressDownButtonPlusEnterButtonOnTheItem(string item)
		{
			IWebElement itemToPress = driver.FindElement(By.CssSelector(item));

			itemToPress.SendKeys(Keys.ArrowDown);

			itemToPress.SendKeys(Keys.Enter);
		}

	}
}
