using AcceptanceTests.Helpers;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Text;

namespace AcceptanceTests.PageObjects
{
	public class EventRelatedItems
	{
		private readonly IWebDriver _driver;

		private readonly IWebElement _tableRowElement;


		public EventRelatedItems(IWebDriver driver, IWebElement webElement)
		{
			_driver = driver;
			_tableRowElement = webElement;
		}

		public string Title => _tableRowElement.Text;
		public IWebElement DeleteRelationButton => _tableRowElement.FindElement(By.XPath("//i[@class='el-tag__close el-icon-close']"));

		public void DeleteRelation()
		{
			DeleteRelationButton.Click();
			_driver.WaitFor(2);
			DeleteRelationButton.Click();
			_driver.WaitFor(1);
		}
		public EventRelatedItems(IWebDriver driver, string value)
		{
			_driver = driver;
			_tableRowElement = driver.FindElement(By.XPath($"//span[contains(text(),'{value}')]"));
		}
	}
}
