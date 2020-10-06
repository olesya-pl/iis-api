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
	public class TextElements
	{
		private readonly ScenarioContext context;
		private readonly IWebDriver driver;

		public TextElements(ScenarioContext injectedContext)
		{
			context = injectedContext;

			driver = context.GetDriver();
		}

		[Then(@"I must see the specific text (.*) in the text (.*) block on the page")]
		public void ThenISeeTheSpecificTextOnThePage(string expectedText, string textBlockName)
		{
			var actualText = driver.FindElement(By.CssSelector(textBlockName)).Text;
			Assert.Contains(expectedText, actualText);

		}

		[Then(@"I must see the (.*) element")]
		public void ThenIMustSeeTheElement(string element)
		{
			IWebElement buttonElement = driver.FindElement(By.CssSelector(element));
			Assert.True(buttonElement.Displayed);
		}
	}
}
