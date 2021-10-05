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

		private IWebElement _deleteButtonElement;

		public MaterialRelatedItems(IWebDriver driver, IWebElement webElement)
		{
			_driver = driver;
			_tableRowElement = webElement;
		}

		public string Title {
			get
            {
				try
                {
					return _tableRowElement.FindElement(By.TagName("b")).Text;
				}
				catch (NoSuchElementException)
                {
					return string.Empty;
                }
			}
		} 
		public IWebElement DeleteRelationButton => _deleteButtonElement;

		public void DeleteRelation()
		{
			DeleteRelationButton.Click();
			_driver.WaitFor(2);
		}

		public MaterialRelatedItems(IWebDriver driver, string value)
		{
			_driver = driver;
			_tableRowElement = driver.FindElement(By.XPath($@"//div[contains(@class, 'el-table__body-wrapper')]//tr//b[contains(text(), '{value}')]"));
			_deleteButtonElement = _driver.FindElement(By.XPath($@"//div[contains(@class, 'el-table__body-wrapper')]//tr//b[contains(text(), '{value}')]//following::button"));
		}
	}
}
