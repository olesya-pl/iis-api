using System;
using AcceptanceTests.Helpers;
using AcceptanceTests.PageObjects;
using Iis.AcceptanceTests.PageObjects.Controls;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using TechTalk.SpecFlow;
using Xunit;

namespace AcceptanceTests.UISteps
{
    [Binding]
    public class ReportSteps
    {

        private readonly IWebDriver driver;
        private readonly ScenarioContext context;

        private ReportPageObjects reportPageObjects;
        private readonly NavigationSection navigationSection;

        public ReportSteps(ScenarioContext injectedContext, IWebDriver driver)
        {
            reportPageObjects = new ReportPageObjects(driver);
            navigationSection = new NavigationSection(driver);
            context = injectedContext;
            this.driver = driver;
        }

        #region When
        [When(@"I navigated to Report section")]
        public void INavigatedToReportSection()
        {
            navigationSection.ReportsLink.Click();
            driver.WaitFor(4);
        }

        [When(@"I pressed the Create a new report button")]
        public void WhenIPressedTheCreateANewReportButton()
        {
            reportPageObjects.CreateNewReportButton.Click();
        }

        [When(@"I entered the (.*) recipient name")]
        public void WhenIEnteredTheRecipientName(string recipientName)
        {
            var reportUniqueName = $"{recipientName} {DateTime.Now.ToLocalTime()} {Guid.NewGuid().ToString("N")}";
            reportPageObjects.ReportCreationAndEdit.EnterRecipientField.SendKeys(recipientName);
            reportPageObjects.ReportCreationAndEdit.EnterRecipientField.SendKeys(Keys.Enter);
        }

        [When(@"I pressed the Proceed button")]
        public void WhenIPressedTheProceedButton()
        {
            reportPageObjects.ReportCreationAndEdit.ProceedButton.Click();
        }

        [When(@"I selected the first event from the event list")]
        public void WhenISelectedTheFirstEventFromTheEventList()
        {
            reportPageObjects.ReportCreationAndEdit.FirstEventCheckboxInTheEventsList.Click();
        }

        [When(@"I pressed Add report button")]
        public void WhenIPressedAddReportButton()
        {
            reportPageObjects.AddEventToReportButton.Click();
        }

        [When(@"I pressed Remove report button")]
        public void WhenIPressedRemoveReportButton()
        {
            driver.WaitFor(0.5);
            reportPageObjects.ReportCreationAndEdit.RemoveEventFromReport.Click();
        }

        [When(@"I selected the first event from the report")]
        public void WhenISelectedTheFirstEventFromTheReport()
        {
            reportPageObjects.ReportCreationAndEdit.FirstEventCheckboxInTheReportsList.Click();
        }

        [When(@"I pressed the Save button")]
        public void WhenIPressedTheSaveButton()
        {
            reportPageObjects.ReportCreationAndEdit.SaveButton.Click();
        }

        [When(@"I pressed the Confirm button")]
        public void WhenIPressedTheConfirmButton()
        {
            reportPageObjects.ReportCreationAndEdit.ConfirmButton.Click();
            driver.WaitFor(3);
        }

        [When(@"I mouse hover on the report")]
        public void WhenIMouseHoverOnTheReport()
        {
            var reportName = context.GetResponse<string>("reportName");
            reportPageObjects.GetReportByTitle(reportName).MouseHoverOnReport();

        }

        [When(@"I entered the (.*) as report title")]
        public void WhenIEnteredTheAsReportTitle(string reportName)
        {
            var reportUniqueName = $"{reportName} {DateTime.Now.ToLocalTime()} {Guid.NewGuid().ToString("N")}";
            context.Set(reportUniqueName, "reportName");
            reportPageObjects.NameOfTheReportField.Clear();
            reportPageObjects.NameOfTheReportField.SendKeys(reportUniqueName);
        }

        [When(@"I clicked two times on the hour and date filter in the report page")]
        public void WhenIClickedTwoTimesOnTheHourAndDateFilterInTheReportPage()
        {
            driver.WaitFor(3);
            reportPageObjects.HourAndDateColumn.Click();
            driver.WaitFor(3);
            reportPageObjects.HourAndDateColumn.Click();
            driver.WaitFor(3);
        }
        #endregion

        #region Then
        [Then(@"I must see first report in the report list")]
        public void IMustSeeFirstReportInTheReportList()
        {
            Assert.True(reportPageObjects.FirstReportInTheReportList.Displayed);
        }

        [Then(@"I must not see an event in the report")]
        public void ThenIMustNotSeeAnEventInTheReport()
        {
            Assert.False(reportPageObjects.IsElementVisible());
        }

        [Then(@"I must see an event in the report")]
        public void ThenIMustSeeAnEventInTheReport()
        {
            Assert.True(reportPageObjects.ReportCreationAndEdit.EventInTheReportList.Displayed);
        }

        [Then(@"I must see a report with specified name")]
        public void ThenIMustSeeAReportWithSpecifiedName()
        {
            var reportName = context.Get<string>("reportName");
            Assert.True(reportPageObjects.GetReportByTitle(reportName).Displayed);
        }

        #endregion
    }
}