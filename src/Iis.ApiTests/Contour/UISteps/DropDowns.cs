using AcceptanceTests.Steps;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Text;
using TechTalk.SpecFlow;

namespace AcceptanceTests.Contour.UISteps
{
	[Binding]
	public class DropDowns
	{
		private readonly ScenarioContext context;
		private readonly IWebDriver driver;

		public DropDowns(ScenarioContext injectedContext)
		{
			context = injectedContext;

			driver = context.GetDriver();
		}

		[Given(@"I select an element (.*) from the (.*) drop down menu")]
		public void GivenISelectAnElementFromTheDropDownMenu(int element, string dropdown)
		{
			SelectElement findDropdownElement = new SelectElement(driver.FindElement(By.CssSelector(dropdown)));

			findDropdownElement.SelectByIndex(element);
			//// select the drop down list
			//var selectDropdown = driver.FindElement(By.CssSelector(dropdown));
			////create select element object 
			//var selectElement = new SelectElement(selectDropdown);

			////select by value
			//selectElement.SelectByValue(element);
			//// select by text
			//selectElement.SelectByText(element);
		}

	}
}
