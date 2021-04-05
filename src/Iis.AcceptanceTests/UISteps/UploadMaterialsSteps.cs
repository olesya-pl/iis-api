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

        public UploadMaterialsSteps(ScenarioContext injectedContext, IWebDriver driver)
        {
            uploadMaterialsPage = new UploadMaterialsPageObjects(driver);
            context = injectedContext;
            this.driver = driver;
            
        }

        #region When
        [When(@"I navigated to the Upload materials page")]
        public void WhenINavigatedToUploadMaterialsPage()
        {
            uploadMaterialsPage.LoadMaterialsSection.Click();
            //driver.WaitFor(0.5);
        }
        #endregion

        #region Then
        [Then(@"I must see choose file for upload button in the Upload materials section")]
        public void ThenIMustSeeChooseFileForUploadButtonInTheUploadMaterialsSection()
        {
            Assert.True(uploadMaterialsPage.UploadMaterialButton.Displayed);
        }
        #endregion
    }
}