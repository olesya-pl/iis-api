using AcceptanceTests.PageObjects;
using OpenQA.Selenium;
using TechTalk.SpecFlow;
using Xunit;

namespace Iis.AcceptanceTests.UISteps
{
    [Binding]
    public class ThemesAndUpdatesSteps
    {
        private readonly IWebDriver driver;
        private readonly ScenarioContext context;

        public ThemesAndUpdatesSteps(ScenarioContext injectedContext, IWebDriver driver)
        {
            context = injectedContext;
            this.driver = driver;
        }

        #region When
        [When(@"I navigated to Themes and updates section")]
        public void INavigatedToThemesAndUpdatesSection()
        {
            var themesAndUpdates = new ThemesAndUpdatesPageObjects(driver);
            themesAndUpdates.ThemesAndUpdatesSection.Click();
        }
        #endregion

        #region Then
        [Then(@"I must see first theme in the Themes list")]
        public void ThenIMustSeeFirstThemeInTheThemeList()
        {
            var themesAndUpdates = new ThemesAndUpdatesPageObjects(driver);
            Assert.True(themesAndUpdates.FirstThemeInTheThemeList.Displayed);
        }
        #endregion
    }
}