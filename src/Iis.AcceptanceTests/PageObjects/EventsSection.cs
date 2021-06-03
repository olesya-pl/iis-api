using Iis.AcceptanceTests.PageObjects.Controls;
using OpenQA.Selenium;
using SeleniumExtras.PageObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AcceptanceTests.PageObjects
{
	class EventsSection
	{
		private readonly IWebDriver driver;

		public EventsSection(IWebDriver driver)
		{
			this.driver = driver;

			PageFactory.InitElements(driver, this);
		}

		[FindsBy(How = How.XPath, Using = "//div[contains(text(),'Події')]")]
		public IWebElement EventsPage;

		[FindsBy(How = How.CssSelector, Using = ".add-button")]
		[CacheLookup]
		public IWebElement CreateEventButton;

		[FindsBy(How = How.CssSelector, Using = ".events-table .p-datatable-tbody > tr")]
		[CacheLookup]
		public IWebElement FirstEventInTheEventsList;

		[FindsBy(How = How.CssSelector, Using = ".el-tooltip")]
		public IWebElement SearchButton;

		[FindsBy(How = How.XPath, Using = "//button[@name='btn-edit']")]
		public IWebElement EditButton;

		[FindsBy(How = How.XPath, Using = "//button[@class='el-button el-tooltip action-button--full-screen el-button--default']")]
		public IWebElement ReviewEventButton;

		[FindsBy(How = How.CssSelector, Using = "[aria-describedby] .el-input__inner")]
		public IWebElement SearchField;

		[FindsBy(How = How.CssSelector, Using = "textarea[name='description']")]
		public IWebElement AdditionalDataTextField;

		public List<Event> GetEventsByName(string eventName)
		{
			return Events.Where(i => i.Name.Contains(eventName)).ToList();
		}

		public List<Event> Events => driver.FindElements(By.ClassName(".events-table tbody.p-datatable-tbody > tr"))
				   .Select(webElement => new Event(driver, webElement)).ToList();
	}
}
