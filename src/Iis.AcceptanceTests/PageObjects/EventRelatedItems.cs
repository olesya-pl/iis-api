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
	}
}
