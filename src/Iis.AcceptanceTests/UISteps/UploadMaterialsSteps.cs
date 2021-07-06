using AcceptanceTests.Helpers;
using AcceptanceTests.PageObjects;
using OpenQA.Selenium;
using TechTalk.SpecFlow;
using Xunit;

namespace AcceptanceTests.UISteps
{
    [Binding]
    public class UploadMaterialsSteps
    {
        private readonly IWebDriver driver;
        private readonly ScenarioContext context;
        private UploadMaterialsPageObjects uploadMaterialsPage;
        private readonly NavigationSection navigationSection;

        public UploadMaterialsSteps(ScenarioContext injectedContext, IWebDriver driver)
        {
            uploadMaterialsPage = new UploadMaterialsPageObjects(driver);
            navigationSection = new NavigationSection(driver);
            context = injectedContext;
            this.driver = driver;
        }

        #region When
        [When(@"I navigated to the Upload materials page")]
        public void WhenINavigatedToUploadMaterialsPage()
        {
            navigationSection.UploaderInputStreamLink.Click();
            //driver.WaitFor(2);
        }
        #endregion

        #region Then
        [Then(@"I must see choose file for upload button in the Upload materials section")]
        public void ThenIMustSeeChooseFileForUploadButtonInTheUploadMaterialsSection()
        {
            Assert.True(uploadMaterialsPage.UploadFunctionalityWindow.Displayed);
        }
        #endregion
    }
}