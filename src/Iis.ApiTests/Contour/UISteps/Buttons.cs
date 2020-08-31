using AcceptanceTests.Steps;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using System;
using System.Collections.Generic;
using System.Text;
using TechTalk.SpecFlow;
using Xunit;

namespace AcceptanceTests.Contour.UISteps
{
	[Binding]
	public class Buttons
	{
		private readonly ScenarioContext context;
		private readonly IWebDriver driver;

		public Buttons(ScenarioContext injectedContext)
		{
			context = injectedContext;

			driver = context.GetDriver();
		}

		#region Given/When
		[Given(@"I click (.*) button")]
		[When(@"I clicked the (.*) button")]
		public void GivenIClickAdministrationMenuItem(string menuItem)
		{
			driver.FindElement(By.CssSelector(menuItem)).Click();
		}

		[When(@"I press the active (.*) button")]
		public void WhenIPressEnterButtonOnTheButton(string button)
		{

			IWebElement buttonToPress = driver.FindElement(By.CssSelector(button));
			driver.SwitchTo().ActiveElement().Click();
		}

		#endregion

		#region Then
		[Then(@"I must see the (.*) button")]
		public void ThenIMustSeeTheButton(string button)
		{
			IWebElement buttonElement = driver.FindElement(By.CssSelector(button));
			Assert.True(buttonElement.Displayed);
		}
		#endregion



	}
}
