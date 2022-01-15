using AcceptanceTests.Helpers;
using AcceptanceTests.PageObjects;
using OpenQA.Selenium;
using TechTalk.SpecFlow;
using Xunit;

namespace AcceptanceTests.UISteps
{
    [Binding]
    public class ReoSteps
    {
        private readonly IWebDriver driver;
        private readonly ScenarioContext context;
        private ReoPageObjects reoPage;
        private readonly NavigationSection navigationSection;

        public ReoSteps(ScenarioContext injectedContext, IWebDriver driver)
        {
            context = injectedContext;
            this.driver = driver;
            reoPage = new ReoPageObjects(driver);
            navigationSection = new NavigationSection(driver);
        }

        #region When

        [When(@"I navigated to Reo page")]
        public void WhenINavigatedToReoPage()
        {
            navigationSection.ReoLink.Click();
            driver.WaitFor(10);
        }


        #endregion When

        #region Then

        [Then(@"I must see Reo block")]
        public void ThenIMustSeeReoBlock()
        {
            Assert.True(reoPage.ReoSearch.Displayed);
        }

        #endregion Then
    }
}