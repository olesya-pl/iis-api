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
        private readonly NavigationSection navigationSection;

        public ThemesAndUpdatesSteps(ScenarioContext injectedContext, IWebDriver driver)
        {
            themesAndUpdatesPageObjects = new ThemesAndUpdatesPageObjects(driver);
            objectsOfStudyPageObjects = new ObjectsOfStudyPageObjects(driver);
            navigationSection = new NavigationSection(driver);
            context = injectedContext;
            this.driver = driver;
        }

        #region When
        [When(@"I navigated to Themes and updates section")]
        public void INavigatedToThemesAndUpdatesSection()
        {
            driver.WaitFor(2);
            navigationSection.ThemesLink.Click();
        }

        [When(@"I pressed on the Create theme button in the objects section")]
        public void WhenIPressedOnTheSaveThemeButtonInTheObjectsSection()
        {
            objectsOfStudyPageObjects.CreateThemeButton.Click();
        }

        [When(@"I entered the (.*) theme name in the objects section")]
        public void WhenIEnteredTheThemeName(string themeName)
        {
            var themeUniqueName = $"{themeName} {DateTime.Now.ToString("dd/MM/yyyy/HH:mm:ss")} {Guid.NewGuid()}";
            context.Set(themeUniqueName, "themeName");
            themesAndUpdatesPageObjects.EnterThemeNameField.SendKeys(themeUniqueName);
            themesAndUpdatesPageObjects.EnterThemeNameField.SendKeys(Keys.Enter);
        }

        [Given(@"I created a theme with a name (.*)")]
        public void GivenICreatedAThemeWithAName(string themeName)
        {
			var themeUniqueName = $"{ themeName} {DateTime.Now.ToString("dd/MM/yyyy/HH:mm:ss")} {Guid.NewGuid()}";
            context.Set(themeUniqueName, themeName);
            objectsOfStudyPageObjects.SearchLoopButton.Click();
            objectsOfStudyPageObjects.SearchField.SendKeys("Попов");
            driver.WaitFor(2);
            objectsOfStudyPageObjects.SearchField.SendKeys(Keys.Down);
            objectsOfStudyPageObjects.SearchField.SendKeys(Keys.Enter);
            objectsOfStudyPageObjects.CreateThemeButton.Click();
            themesAndUpdatesPageObjects.EnterThemeNameField.SendKeys(themeUniqueName);
            themesAndUpdatesPageObjects.EnterThemeNameField.SendKeys(Keys.Enter);
        }

        [When(@"I Delete theme (.*)")]
        public void WhenIDeleteTheme(string themeName)
        {
            var themeUniqueName = context.Get<string>(themeName);
            var theme = themesAndUpdatesPageObjects.GetThemeByTitle(themeUniqueName);
            theme.DeleteTheme();
        }

        #endregion

        #region Then
        [Then(@"I must see first theme in the Themes list")]
        public void ThenIMustSeeFirstThemeInTheThemeList()
        {
            driver.WaitFor(2);
            Assert.True(themesAndUpdatesPageObjects.FirstThemeInTheThemeList.Displayed);
        }

        [Then(@"I must see a theme with specified name")]
        public void ThenIMustSeeAThemeWithASpecifiedName()
        {
            driver.WaitFor(7);
            var list = themesAndUpdatesPageObjects.Themes.First().Title;
            var themeName = context.Get<string>("themeName");
            Assert.True(themesAndUpdatesPageObjects.GetThemeByTitle(themeName).Displayed);
        }

        [Then(@"I must not see a theme (.*)")]
        public void ThenIMustNotSeeATheme(string themeName)
        {
            var themeUniqueName = context.Get<string>(themeName);
            Assert.True(themesAndUpdatesPageObjects.Themes.Count(_ => _.Title == themeUniqueName) == 0);
        }
        #endregion         
    }
}