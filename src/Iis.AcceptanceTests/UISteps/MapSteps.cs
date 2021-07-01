using AcceptanceTests.Helpers;
using AcceptanceTests.PageObjects;
using OpenQA.Selenium;
using TechTalk.SpecFlow;
using Xunit;

namespace AcceptanceTests.UISteps
{
    [Binding]
    public class MapSteps
    {
        private readonly IWebDriver driver;
        private readonly ScenarioContext context;
        private MapPageObjects mapPage;
        private readonly NavigationSection navigationSection;

        public MapSteps(ScenarioContext injectedContext, IWebDriver driver)
        {
            context = injectedContext;
            this.driver = driver;
            mapPage = new MapPageObjects(driver);
            navigationSection = new NavigationSection(driver);
        }

        #region When
        [When(@"I navigated to Map page")]
        public void INavigatedToMapPage()
        {
            navigationSection.MapLink.Click();
            driver.WaitFor(10);
        }
        #endregion

        #region Then
        [Then(@"I must see Map block")]
        public void IMustSeeMapBlock()
        {
            Assert.True(mapPage.MapSearch.Displayed);
        }
        #endregion
    }
}