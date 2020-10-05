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
	public class TextFields
	{
		private readonly ScenarioContext context;
		private readonly IWebDriver driver;

		public TextFields(ScenarioContext injectedContext)
		{
			context = injectedContext;

			driver = context.GetDriver();
		}

		[Given(@"I enter (.*) in the (.*) text field and add current date to the input")]
		public void GivenIEnterSomethingInTheTextField(string text, string textField)
		{
			IWebElement textForm = driver.FindElement(By.CssSelector(textField));
			///driver.SwitchTo().ActiveElement().SendKeys($"{text}_{DateTime.Now}");
			///driver.SwitchTo().DefaultContent();
			textForm.SendKeys($"{text}_{DateTime.Now}");		
		}

		[Given(@"I input (.*) in the (.*) text field")]
		public void GivenIInputInTheTextField(string text, string textField)
		{
			IWebElement textForm = driver.FindElement(By.CssSelector(textField));

			textForm.SendKeys(text);
		}


		[Then(@"the text field (.*) and text field (.*) must be highlighted with red color")]
		public void ThenTheFieldAndFieldMustBeHighlightedWithRedColor(string loginField, string passwordField)
		{
			IWebElement expectedLoginField = driver.FindElement(By.CssSelector(loginField));
			IWebElement expectedPasswordField = driver.FindElement(By.CssSelector(passwordField));

			Assert.True(expectedLoginField.Displayed);
			Assert.True(expectedPasswordField.Displayed);
		}



	}
}
