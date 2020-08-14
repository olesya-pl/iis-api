using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using TechTalk.SpecFlow;

namespace AcceptanceTests.Contour.UISteps
{
	[Binding]
	public sealed class Administration : AuthorizationUI
	{
		private readonly ScenarioContext context;

		public Administration(ScenarioContext injectedContext) : base (injectedContext)
		{
			context = injectedContext;

		}

		[Given(@"I click (.*) button")]
		public void GivenIClickAdministrationMenuItem(string menuItem)
		{
			driver.FindElement(By.CssSelector(menuItem)).Click();
		}

		[Given(@"I complete the userform with (.*), (.*), (.*), (.*), (.*) and (.*)")]
		public void FillTheGodDamnForm(string firstname, string lastname, string patronym, string username, string password, string conformationPassword)
		{
			driver.FindElement(By.CssSelector(".el-form-item label[for=firstName]+.el-form-item__content input")).SendKeys(firstname);
			driver.FindElement(By.CssSelector(".el-form-item label[for=lastName]+.el-form-item__content input")).SendKeys(lastname);
			driver.FindElement(By.CssSelector(".el-form-item label[for=patronymic]+.el-form-item__content input")).SendKeys(patronym);
			driver.FindElement(By.CssSelector(".el-form-item label[for=userName]+.el-form-item__content input")).SendKeys(username + DateTime.Now.GetHashCode());
			driver.FindElement(By.CssSelector(".el-form-item label[for=password]+.el-form-item__content input")).SendKeys(password);
			Thread.Sleep(3000);
			driver.FindElement(By.CssSelector(".el-form-item label[for=passwordConfirmation]+.el-form-item__content input")).SendKeys(conformationPassword);
		}

		[Given(@"I choose element from dropdown menu")]
		public void GivenIChooseElementFromDropdownMenu()
		{
			 driver.FindElement(By.CssSelector("div[class='el-select'] input")).Click();
			 Thread.Sleep(1000);
			 driver.FindElement(By.XPath("/html/body/div[3]/div[1]/div[1]/ul/li[1]/span")).Click();
	    }
	}
}
