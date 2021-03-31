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
        private UploadMaterialsPageObjects uploadMaterialsPageObjects;

        public UploadMaterialsSteps(ScenarioContext injectedContext, IWebDriver driver)
        {
            uploadMaterialsPageObjects = new UploadMaterialsPageObjects(driver);
            context = injectedContext;
            this.driver = driver;
            
        }

        #region When
        [When(@"I navigated to the Upload materials page")]
        public void WhenINavigatedToUploadMaterialsPage()
        {
            var uploadMaterialsPage = new UploadMaterialsPageObjects(driver);
            uploadMaterialsPage.LoadMaterialsSection.Click();
        }
        #endregion

        #region Then
        [Then(@"I must see choose file for upload button in the Upload materials section")]
        public void ThenIMustSeeChooseFileForUploadButtonInTheUploadMaterialsSection()
        {
            Assert.True(uploadMaterialsPageObjects.UploadMaterialButton.Displayed);
        }
        #endregion
    }
}