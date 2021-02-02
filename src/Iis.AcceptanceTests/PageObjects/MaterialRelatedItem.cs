﻿using AcceptanceTests.Helpers;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Text;

namespace AcceptanceTests.PageObjects
{
	public class MaterialRelatedItem
	{
		private readonly IWebDriver _driver;

		private readonly IWebElement _tableRowElement;


		public MaterialRelatedItem(IWebDriver driver, IWebElement webElement)
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

		public MaterialRelatedItem(IWebDriver driver, string value)
		{
			_driver = driver;
			_tableRowElement = driver.FindElement(By.XPath($@"//table[@class='el-table__body']//tr/td[1]//b[.='{value}']"));
		}
	}
}
