using OpenQA.Selenium;
using SeleniumExtras.PageObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace AcceptanceTests.PageObjects
{
	public class WikiSectionPageObjects
	{
		private readonly IWebDriver driver;

		public WikiSectionPageObjects(IWebDriver driver)
		{
			this.driver = driver;
			PageFactory.InitElements(driver, this);
		}

		[FindsBy(How = How.CssSelector, Using = ".sidebar__nav li.wiki")]
		public IWebElement WikiSection;

		[FindsBy(How = How.XPath, Using = "//div[@class='infinity-table wiki-table']//tbody[@class='p-datatable-tbody']/tr[1]")]
		// [FindsBy(How = How.CssSelector, Using = ".wiki-table .p-datatable-tbody > tr")]
		[CacheLookup]
		public IWebElement FirstWikiObjectInTheObjectsList;
	}
}
