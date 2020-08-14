﻿using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System.Threading;
using TechTalk.SpecFlow;
using AcceptanceTests.Steps;

namespace AcceptanceTests.Contour.UISteps
{
	[Binding]
	public class PopUps
	{
		private readonly ScenarioContext context;
		private readonly IWebDriver driver;

		public PopUps(ScenarioContext injectedContext)
		{
			context = injectedContext;
			
			driver = context.GetDriver();
		}

		[Given(@"I select an element (.*) in the (.*) pop up")]
		[When(@"I selected an element (.*) in the (.*) pop up")]
		public void GivenISelectAnElementInThePopUp(string element, string popup)
		{
			driver.FindElement(By.CssSelector(popup)).FindElement(By.CssSelector(element)).Click();
		}

	}
}
