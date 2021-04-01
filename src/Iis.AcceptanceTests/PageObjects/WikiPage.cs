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

		[FindsBy(How = How.XPath, Using = "//div[contains(text(),'Довідник ОіВТ')]")]
		public IWebElement WikiSection;

		[FindsBy(How = How.XPath, Using = "//table[@class='el-table__body']/tbody/tr[1]")]
		public IWebElement FirstWikiObjectInTheObjectsList;
	}
}
