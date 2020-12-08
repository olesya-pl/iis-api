using AcceptanceTests.PageObjects;
using OpenQA.Selenium;
using TechTalk.SpecFlow;
using Xunit;

namespace AcceptanceTests.UISteps
{
    [Binding]
    public class ReportSteps
    {

        private readonly IWebDriver driver;
        private readonly ScenarioContext context;

        public ReportSteps(ScenarioContext injectedContext, IWebDriver driver)
        {
            context = injectedContext;
            this.driver = driver;
        }

        #region When 
        [When(@"I navigated to Report section")]
        public void INavigatedToReportSection()
        {
            var reportPage = new ReportPageObjects(driver);
            reportPage.ReportSection.Click();
        }
        #endregion

        #region Then
        [Then(@"I must see first report in the report list")]
        public void IMustSeeFirstReportInTheReportList()
        {
            var reportPage = new ReportPageObjects(driver);
            Assert.True(reportPage.FirstReportInTheReportList.Displayed);
        }
        #endregion

    }

}