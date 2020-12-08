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

        public MapSteps(ScenarioContext injectedContext, IWebDriver driver)
        {
            context = injectedContext;
            this.driver = driver;
        }

        #region When

        [When(@"I navigated to Map page")]
        public void INavigatedToMapPage()
        {

            var mapPage = new MapPageObjects(driver);
            mapPage.MapSection.Click();
        }

        #endregion

        #region Then

        [Then(@"I must see Map block")]
        public void IMustSeeMapBlock()
        {
            var mapPage = new MapPageObjects(driver);
            Assert.True(mapPage.MapSearch.Displayed);
        }

        #endregion
    }
}