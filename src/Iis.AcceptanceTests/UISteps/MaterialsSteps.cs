using System;
using System.Linq;
using System.Threading.Tasks;
using AcceptanceTests.Helpers;
using AcceptanceTests.PageObjects;
using OpenQA.Selenium;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Xunit;

namespace AcceptanceTests.UISteps
{
    [Binding]
    public class MaterialsSteps
    {
        private readonly IWebDriver driver;
        private readonly ScenarioContext context;
        private readonly MaterialsSectionPage materialsSectionPage;
        private readonly NavigationSection navigationSection;

        public MaterialsSteps(ScenarioContext injectedContext, IWebDriver driver)
        {
            materialsSectionPage = new MaterialsSectionPage(driver);
            navigationSection = new NavigationSection(driver);

            context = injectedContext;
            this.driver = driver;
        }

        #region When
        [When(@"I navigated to Materials page")]
        public void IWantNavigateToMaterialsPage()
        {
            navigationSection.MaterialsLink.Click();
            driver.WaitFor(7);
        }

        [When(@"I clicked on the first material in the Materials list")]
        public void IClieckedOnTheFirstMaterialInTheMaterialsList()
        {
            driver.WaitFor(2);
            materialsSectionPage.FirstMaterialInTheMaterialsList.Click();
            driver.WaitFor(1);
        }

        [When(@"I clicked on the relations tab in the material card")]
        public void IClickedOnTheEventsTabInTheMaterialCard()
        {
            driver.WaitFor(2);
            materialsSectionPage.RelationsTab.Click();
            driver.WaitFor(1);
        }

        [When(@"I clicked on the relation tab in the material card")]
        public void IClickedOnTheObjectsTabInTheMaterialCard()
        {
            driver.WaitFor(2);
            materialsSectionPage.RelationsTab.Click();
        }

        [When(@"I clicked on the ML tab in the material card")]
        public void IClickedOnTheMLTabInTheMaterialCard()
        {
            materialsSectionPage.MLTab.Click();
        }

        [When(@"I got search counter value in the Materials section")]
        public void IGetSearchCounterValueInTheMaterialsSection()
        {
            context.SetResponse("counterValue", materialsSectionPage.MaterialsSearchResultCounter.Text);
        }

        [When(@"I clicked search button in the Materials section")]
        public void IFilledInfoInTheMaterialsSearchField()
        {
            materialsSectionPage.SearchButton.Click();
        }

        [When(@"I searched (.*) data in the materials")]
        public void IEnteredDataInTheSearchField(string input)
        {
            materialsSectionPage.SearchField.SendKeys(input);
            driver.WaitFor(2);
            materialsSectionPage.SearchField.SendKeys(Keys.Enter);
            driver.WaitFor(5);
        }

        [When(@"I searched for uploaded material in the materials")]
        public void WhenISearchedForUploadedMaterialInTheMaterials()
        {
            materialsSectionPage.SearchField.SendKeys(context.Get<string>("uploadedMaterial"));
            driver.WaitFor(2);
            materialsSectionPage.SearchField.SendKeys(Keys.Enter);
            driver.WaitFor(5);
        }

        [Given(@"I upload a new docx material via API")]
        public async Task GivenIUploadANewDocxMaterialViaAPI(Table table)
        {
            var uniqueSuffix = Guid.NewGuid().ToString();
            var materialModel = table.CreateInstance<MaterialModel>();
            materialModel.FileName = materialModel.FileName + uniqueSuffix;
            materialModel.Content = materialModel.Content + uniqueSuffix;
            context.Set(materialModel.FileName, "uploadedMaterial");
            await MaterialsHelper.UploadDocxMaterial(materialModel);
        }

        [When(@"I set importance (.*) value")]
        public void WhenIClickedOnTheDropDownMenuInTheMaterials(string value)
        {
            materialsSectionPage.MaterialPage.ImportanceDropDown.Select(value);
        }

        [When(@"I set reliability (.*) value")]
        public void WhenISetPriority(string priority)
        {
            materialsSectionPage.MaterialPage.ReliabilityDropDown.Select(priority);
        }

        //[When(@"I set the session priority (.*) value")]
        //public void WhenISetTheSessionPriorityValue(string sessionPriority)
        //{
        //    materialsSectionPage.MaterialPage.SessionPriorityDropDown.Select(sessionPriority);
        //}

        [When(@"I set the session priority to Important")]
        public void WhenISetTheSessionPriorityValueImportant()
        {
            materialsSectionPage.MaterialImportantButton.Click();
        }

        [When(@"I set the session priority to Immediate Report")]
        public void WhenISetTheSessionPriorityValueImmediateReport()
        {
            materialsSectionPage.MaterialImmediateReportButton.Click();
        }

        [When(@"I set the session priority to Translation")]
        public void WhenISetTheSessionPriorityValueTranslation()
        {
            materialsSectionPage.MaterialTranslationButton.Click();
        }

        [When(@"I set the source credibility (.*) value")]
        public void WhenISetTheSourceCredibility(string sourceCredibility)
        {
            materialsSectionPage.MaterialPage.SourceCredibilityDropDown.Select(sourceCredibility);
        }


        [When(@"I pressed Processed button")]
        public void WhenIPressProcessedButton()
        {
            materialsSectionPage.ProcessedButton.Click();
            driver.WaitFor(15);
        }

        [Then(@"I pressed Processed button")]
        public void ThenIPressProcessedButton()
        {
            materialsSectionPage.ProcessedButton.Click();
            driver.WaitFor(15);
        }

        [When(@"I pressed Back button")]
        public void WhenIPressBackButton()
        {
            driver.Navigate().Back();
            driver.WaitFor(2);
        }

        [When(@"I pressed the Previous material button")]
        public void WhenIPressedThePreviousButton()
        {
            materialsSectionPage.PreviousMaterialButton.Click();
            driver.WaitFor(2);
        }

        [When(@"I clicked on the first search result in the Materials section")]
        public void WhenIClickedOnTheFirstSearchResultInTheMaterialsSection()
        {
            materialsSectionPage.FirstSearchResult.Click();
            driver.WaitFor(5);
        }

        [When(@"I pressed Show button to show Text classifier ML output")]
        public void WhenIPressedShowButtonToShowTextClassifierMLOutput()
        {
            materialsSectionPage.TextClassifierMLOutputButton.Click();
        }

        [When(@"I enter (.*) value in the search object field")]
        public void WhenIEnterValueInTheSearchObjectField(string inputValue)
        {
            materialsSectionPage.ObjectsTabSearch.SendKeys(inputValue);
            driver.WaitFor(2);
            materialsSectionPage.ObjectsTabSearch.SendKeys(Keys.Down);
            materialsSectionPage.ObjectsTabSearch.SendKeys(Keys.Enter);
            materialsSectionPage.ObjectsTabSearch.SendKeys(Keys.Escape);
        }

        [When(@"I clicked on the connected object")]
        public void WhenIClickedOnTheConnectedObject()
        {
            materialsSectionPage.ConnectedObjectLink.Click();
        }

        [When(@"I clicked Back button in the browser")]
        public void WhenIClickedBackButtonInTheBrowser()
        {
            driver.Navigate().Back();
        }

        [When(@"I clicked on the delete button to destroy relation between the material and the (.*) object")]
        public void WhenIClickedDeleteRelatedObjectFromTheMaterial(string objectName)
        {
            var objectToUnlink = materialsSectionPage.GetItemTitleRelatedToMaterial(objectName);
            objectToUnlink.DeleteRelation();
        }

        [When(@"I clicked on the delete button to destroy relation between the material and the (.*) event")]
        public void WhenIClickedOnTheDeleteButtonToDestroyRelationBetweenTheMaterialAndTheEvent(string eventName)
        {
            var eventUniqueName = context.Get<string>(eventName);
            var eventToUnlink = materialsSectionPage.GetItemTitleRelatedToMaterial(eventUniqueName);
            eventToUnlink.DeleteRelation();
        }


        [When(@"I refreshed the page in the browser")]
        public void WhenIRefreshedThePageInTheBrowser()
        {
            driver.Navigate().Refresh();
        }

        [When(@"I pressed the confirm button")]
        public void WhenIPressedConfirmButton()
        {
            materialsSectionPage.ConfirmDeleteRelationButton.Click();
            driver.WaitFor(3);

        }

        [When(@"I scrolled down to the bottom of the relations tab")]
        public void WhenIScrolledDownToTheElementSearchFiledInTheRelationsTab()
        {
            materialsSectionPage.ScrollToEnd();
        }

        [When(@"I clicked on the pattern tab")]
        public void WhenIClickedOnThePatternTab()
        {
            materialsSectionPage.PatternTab.Click();
        }

        [When(@"I binded the (.*) event to the material")]
        public void WhenIBindedTheEventToTheMaterial(string eventName)
        {
            var eventUniqueName = context.Get<string>(eventName);
            materialsSectionPage.EventsSearch.SendKeys($"\"{eventUniqueName}\"");
            driver.WaitFor(1);
            materialsSectionPage.EventsSearch.SendKeys(Keys.Down);
            materialsSectionPage.EventsSearch.SendKeys(Keys.Enter);
            materialsSectionPage.EventsSearch.SendKeys(Keys.Escape);
            driver.WaitFor(2);
        }

        [When(@"I clicked on the clear search button")]
        public void WhenIClickedOnTheClearSearchButton()
        {
            materialsSectionPage.ClearSearchFieldButton.Click();
            driver.WaitFor(1);
        }

        [When(@"I close the material card")]
        public void ThenICloseTheMaterialCard()
        {
            materialsSectionPage.CloseMaterialCardButton.Click();
            driver.WaitFor(5);
        }


        #endregion When

        #region Then
        [Then(@"I must see the Materials page")]
        public void ThenIMustSeeMaterialsPage()
        {
            Assert.Contains("input-stream/?sort=createdDate_desc&page=1", driver.Url);
        }

        [Then(@"I must see first material in the Materials list")]
        public void ThenIMustSeeFirstMaterialInTheMaterialsList()
        {

            Assert.True(materialsSectionPage.FirstMaterialInTheMaterialsList.Displayed);
        }

        [Then(@"I must see processed button in the materials card")]
        public void IMustSeeProcessedButtonOnTheMaterialCard()
        {

            Assert.True(materialsSectionPage.ProcessedButton.Displayed);
        }

        [Then(@"I must see events search in the materials card")]
        public void IMustSeeEventsSearchInTheMaterialsCard()
        {

            Assert.True(materialsSectionPage.EventsSearch.Displayed);
        }

        [Then(@"I must see objects search in the materials card")]
        public void IMustSeeObjectsSearchInTheMaterialsCard()
        {
            Assert.True(materialsSectionPage.ObjectsTabSearch.Displayed);
        }

        [Then(@"I must see relevance drop down in the materials card")]
        public void IMustSeeRelevanceDropDownInTheMaterialsCard()
        {
            Assert.True(materialsSectionPage.RelevanceDropDown.Displayed);
        }

        [Then(@"I must see these elements")]
        public void IMustSeeTheseDropDownElements(Table table)
        {
            foreach (TableRow row in table.Rows)
            {
                var tableValue = row.Values.First();
                Assert.True((typeof(MaterialsSectionPage).GetField(tableValue).GetValue(materialsSectionPage) as IWebElement).Displayed);
            }
        }

        [Then(@"I must I must see at least one user in the originator drop down menu")]
        public void IMustSeeAtLeastOneUserInTheOriginatorDropDonwMenu()
        {
            materialsSectionPage.Originator.Click();
            var list = driver.FindElements(By.ClassName("el-select-dropdown__item"));
            Assert.True(list.Count() > 0);
        }

        [Then(@"I must see zero results in the Materials section")]
        public void ThenIMustSeeZeroResults()
        {
            Assert.True(materialsSectionPage.EmptySearchField.Displayed);
        }

        [Then(@"I must see Show button in the ML tab")]
        public void ThenIMustSeeShowButtonInTheMLTab()
        {
            Assert.True(materialsSectionPage.ShowMLResultsButton.Displayed);
        }

        [Then(@"I must see that search counter values are equal in the Materials section")]
        public void ThenIMustSeeThatSearchCounterValuesAreEqualInTheMaterialsSection()
        {
            string actualValue = materialsSectionPage.MaterialsSearchResultCounter.Text;
            string expectedValue = context.GetResponse<string>("counterValue");
            Assert.Equal(actualValue, expectedValue);
        }

        [Then(@"I must see that importance value must be set to (.*) value")]
        public void ThemIMustSeeThatImportanceValueMustBeSetToValue(string expectedValue)
        {
            var actualValue = materialsSectionPage.MaterialPage.ImportanceDropDown.Text;
            Assert.Equal(expectedValue, actualValue);
        }

        [Then(@"I must see that reliability value must be set to (.*) value")]
        public void ThemIMustSeeThatRelevanceValueMustBeSetToValue(string expectedValue)
        {
            var actualValue = materialsSectionPage.MaterialPage.ReliabilityDropDown.Text;
            Assert.Equal(expectedValue, actualValue);
        }

        //[Then(@"I must see that the session priority value must be set to the (.*) value")]
        //public void ThenIMustSeeThatTheSessionPriorityValueMustBeSetToTheValue(string expectedValue)
        //{
        //    var actualValue = materialsSectionPage.MaterialPage.SessionPriorityDropDown.Text;
        //    Assert.Equal(expectedValue, actualValue);
        //}

        [Then(@"I must see that the session priority value must be set to Important")]
        public void ThenIMustSeeThatTheSessionPriorityValueMustBeSetToTheImportantValue()
        {
            materialsSectionPage.MaterialImportantButton.HasClass("selected");
        }

        [Then(@"I must see that the session priority value must be set to Immediate Report")]
        public void ThenIMustSeeThatTheSessionPriorityValueMustBeSetToTheImmediateReportValue()
        {
            materialsSectionPage.MaterialImmediateReportButton.HasClass("selected");
        }

        [Then(@"I must see that the session priority value must be set to Translation")]
        public void ThenIMustSeeThatTheSessionPriorityValueMustBeSetToTheTranslationValue()
        {
            materialsSectionPage.MaterialTranslationButton.HasClass("selected");
        }

        [Then(@"I must see that the source credibility value must be set to the (.*) value")]
        public void ThenIMustSeeThatTheSourceCredibilityValueMustBeSetToTheValue(string expectedValue)
        {
            var actualValue = materialsSectionPage.MaterialPage.SourceCredibilityDropDown.Text;
            Assert.Equal(expectedValue, actualValue);
        }

        [Then(@"I must see Text classifier ML output form")]
        public void IMustSeeTextClassifierMLOutPutFor()
        {
            Assert.True(materialsSectionPage.TextClassifierMLOutputForm.Displayed);
        }

        [Then(@"I must see a material that contains (.*) word in the Materials search result")]
        public void ThenIMustSeeAMaterialThatContainsWordInTheMaterialSearchResult(string keyword)
        {
            var actualContentText = materialsSectionPage.FirstSearchResultContentBlock.Text;
            Assert.Contains(keyword, actualContentText);
        }

        [Then(@"I must not see the related (.*) object in the material")]
        public void ThenIMustNotSeeTheRelatedObjectInTheMaterial(string objectName)
        {
            Assert.DoesNotContain(materialsSectionPage.MaterialsRelatedObjects, _ => _.Title == objectName);
        }

        [Then(@"I must see that phone number pattern is equal to value")]
        public void ThenIMustSeeThatPhoneNumberPatternIsEqualToValue(string phoneNumber)
        {
            phoneNumber = phoneNumber.Trim();
            var actualPhoneNumber = materialsSectionPage.PhoneNumberPatternNode.Text;
            Assert.Equal(phoneNumber, actualPhoneNumber);
        }

        [Then(@"I must not see (.*) as the related event to the material")]
        public void ThenIMustNotSeeТестоваПодіяAsTheRelatedEventToTheMaterial(string eventName)
        {
            var eventUniqueName = context.Get<string>(eventName);
            Assert.DoesNotContain(materialsSectionPage.MaterialsRelatedEvents, _ => _.Title == eventUniqueName);
        }

        [Then(@"I must see (.*) as the related event to the material")]
        public void ThenIMustSeeТестоваПодіяAsTheRelatedEventToTheMaterial(string eventName)
        {
            var eventUniqueName = context.Get<string>(eventName);
            Assert.Contains(materialsSectionPage.MaterialsRelatedEvents, _ => _.Title == eventUniqueName);
        }

        [Then(@"I must see the (.*) title of the material")]
        public void ThenIMustSeeTheTitleOfTheMaterial(string expectedMaterialName)
        {
            var actualMaterialName = materialsSectionPage.MaterialTitle.Text;
            Assert.Contains(expectedMaterialName, actualMaterialName);
        }
        #endregion
    }
}