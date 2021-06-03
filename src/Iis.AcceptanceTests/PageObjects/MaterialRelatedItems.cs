using AcceptanceTests.Helpers;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Text;

namespace AcceptanceTests.PageObjects
{
	public class MaterialRelatedItems
	{
		private readonly IWebDriver _driver;

		private readonly IWebElement _tableRowElement;

		public MaterialRelatedItems(IWebDriver driver, IWebElement webElement)
		{
			_driver = driver;
			_tableRowElement = webElement;
		}

		public string Title => _tableRowElement.FindElement(By.TagName("b")).Text;

		public IWebElement DeleteRelationButton => _tableRowElement.FindElement(By.XPath("//button[@name='delete']"));

		public void DeleteRelation()
		{
			DeleteRelationButton.Click();
			_driver.WaitFor(2);
		}

		public MaterialRelatedItems(IWebDriver driver, string value)
		{
			_driver = driver;
			_tableRowElement = driver.FindElement(By.XPath($@"//div[@class='material-objects']//div[@class='el-table__body-wrapper is-scrolling-none']//tr//b[contains(text(), '{value}')]//ancestor::tr"));
		}
	}
}
