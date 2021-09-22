using AcceptanceTests.PageObjects;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Text;
using AcceptanceTests.Helpers;
using TechTalk.SpecFlow;
using Xunit;

namespace AcceptanceTests.UISteps
{
	[Binding]
	public class WikiSteps
	{
		private readonly IWebDriver driver;
		private readonly ScenarioContext context;
		private WikiSectionPageObjects wikiPage;
		private readonly NavigationSection navigationSection;

		public WikiSteps(ScenarioContext injectedContext, IWebDriver driver)
		{
			wikiPage = new WikiSectionPageObjects(driver);
			navigationSection = new NavigationSection(driver);
			context = injectedContext;
			this.driver = driver;
		}

		[When(@"I navigated to the Wiki page")]
		public void WhenINavigatedToTheWikiPage()
		{
			navigationSection.WikiLink.Click();
			driver.WaitFor(5);
		}

		[Then(@"I must see the Wiki page")]
		public void ThenIMustSeeTheWikiPage()
		{
			Assert.True(wikiPage.FirstWikiObjectInTheObjectsList.Displayed);
		}


	}
}
