using System;
using System.Linq;
using AcceptanceTests.Helpers;
using AcceptanceTests.PageObjects;
using OpenQA.Selenium;
using TechTalk.SpecFlow;
using Xunit;

namespace AcceptanceTests.UISteps
{
    [Binding]
    public class ThemesAndUpdatesSteps
    {
        private readonly IWebDriver driver;
        private readonly ScenarioContext context;

        private ThemesAndUpdatesPageObjects themesAndUpdatesPageObjects;

        private ObjectsOfStudyPageObjects objectsOfStudyPageObjects;
        private MaterialsSectionPage materialsSectionPage;

        public ThemesAndUpdatesSteps(ScenarioContext injectedContext, IWebDriver driver)
        {
            themesAndUpdatesPageObjects = new ThemesAndUpdatesPageObjects(driver);
            objectsOfStudyPageObjects = new ObjectsOfStudyPageObjects(driver);
            materialsSectionPage = new MaterialsSectionPage(driver);
            context = injectedContext;
            this.driver = driver;
        }

        #region When
        [When(@"I navigated to Themes and updates section")]
        public void INavigatedToThemesAndUpdatesSection()
        {
            themesAndUpdatesPageObjects.ThemesAndUpdatesSection.Click();
        }

        [When(@"I pressed on the Create theme button in the objects section")]
        public void WhenIPressedOnTheSaveThemeButtonInTheObjectsSection()
        {
            objectsOfStudyPageObjects.CreateThemeButton.Click();
        }

        [When(@"I entered the (.*) theme name in the objects section")]
        public void WhenIEnteredTheThemeName(string themeName)
        {
            var themeUniqueName = $"{themeName} {DateTime.Now.ToLocalTime()} {Guid.NewGuid().ToString("N")}";
            context.Set(themeUniqueName, "themeName");
            themesAndUpdatesPageObjects.EnterThemeNameField.SendKeys(themeUniqueName);
            themesAndUpdatesPageObjects.EnterThemeNameField.SendKeys(Keys.Enter);
        }

        #endregion

        #region Then
        [Then(@"I must see first theme in the Themes list")]
        public void ThenIMustSeeFirstThemeInTheThemeList()
        {
            Assert.True(themesAndUpdatesPageObjects.FirstThemeInTheThemeList.Displayed);
        }

        [Then(@"I must see a theme with specified name")]
        public void ThenIMustSeeAThemeWithASpecifiedName()
        {
            var list = themesAndUpdatesPageObjects.Themes.First().Title;
            var themeName = context.Get<string>("themeName");
            Assert.True(themesAndUpdatesPageObjects.GetThemeByTitle(themeName).Displayed);
        }
        #endregion

        [Given(@"I created a theme with a name (.*)")]
        public void GivenICreatedAThemeWithAName(string themeName)
        {
            var themeUniqueName = themeName + Guid.NewGuid();
            context.Set(themeUniqueName, themeName);
            objectsOfStudyPageObjects.SearchLoopButton.Click();
            materialsSectionPage.ObjectsTabSearch.SendKeys("Попов");
            driver.WaitFor(2);
            materialsSectionPage.ObjectsTabSearch.SendKeys(Keys.Down);
            materialsSectionPage.ObjectsTabSearch.SendKeys(Keys.Enter);
            objectsOfStudyPageObjects.CreateThemeButton.Click();
            themesAndUpdatesPageObjects.EnterThemeNameField.SendKeys(themeUniqueName);
            themesAndUpdatesPageObjects.EnterThemeNameField.SendKeys(Keys.Enter);
        }

        [When(@"I Delete theme (.*)")]
        public void WhenIDeleteTheme(string themeName)
        {
            var themeUniqueName = context.Get<string>(themeName);
            driver.WaitFor(15);
            var theme = themesAndUpdatesPageObjects.GetThemeByTitle(themeUniqueName);
            theme.DeleteTheme();
        }
        [Then(@"I must not see a theme (.*)")]
        public void ThenIMustNotSeeATheme(string themeName)
        {
            driver.WaitFor(15);
            var themeUniqueName = context.Get<string>(themeName);
            Assert.True(themesAndUpdatesPageObjects.Themes.Count(_ => _.Title == themeUniqueName) == 0);
        }
    }
}