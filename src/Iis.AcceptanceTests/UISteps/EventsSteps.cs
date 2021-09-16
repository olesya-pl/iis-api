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
    public class EventsSteps
    {
        private readonly IWebDriver driver;
        private readonly ScenarioContext context;

        private EventPage eventPage;
        private EventsSection eventsSection;
        private readonly NavigationSection navigationSection;

        #region Given/When
        public EventsSteps(ScenarioContext injectedContext, IWebDriver driver)
        {
            eventPage = new EventPage(driver);
            eventsSection = new EventsSection(driver);
            navigationSection = new NavigationSection(driver);
            context = injectedContext;
            this.driver = driver;
        }

        [When(@"I navigated to Events page")]
        public void IWantNavigateToEventsPage()
        {
            navigationSection.EventsLink.Click();
            driver.WaitFor(3);
        }

        [When(@"I created a new (.*) event")]
        public void WhenICreatedANewEvent(string eventName)
        {
            var eventUniqueName = $"{eventName} {DateTime.Now.ToLocalTime()} {Guid.NewGuid().ToString("N")}";
            context.Set(eventUniqueName, eventName);
            eventsSection.CreateEventButton.Click();
            eventPage.EventTitle.SendKeys(eventUniqueName);
            eventPage.AverageImportaceRadioButton.Click();
            eventPage.FlawRadioButton.Click();
            eventPage.SecurityClassificationField.SendKeys("НВ");
            eventPage.SecurityClassificationField.SendKeys(Keys.Down);
            eventPage.SecurityClassificationField.SendKeys(Keys.Enter);
            eventPage.CountrySelectionDropDown.Click();
            var countryInput = eventPage.CountrySelectionDropDown.FindElement(By.TagName("input"));
            countryInput.SendKeys("Росія");
            driver.WaitFor(2);
            countryInput.SendKeys(Keys.Down);
            countryInput.SendKeys(Keys.Enter);
            eventPage.EventTypeDropDown.Click();
            driver.WaitFor(2);
            eventPage.EventTypeDropDown.SendKeys(Keys.Up);
            eventPage.EventTypeDropDown.SendKeys(Keys.Enter);
            eventPage.EventComponentDropDown.SendKeys("Кризові регіони");
            eventPage.EventComponentDropDown.SendKeys(Keys.Down);
            eventPage.EventComponentDropDown.SendKeys(Keys.Enter);
            eventPage.SaveEventChangesButton.Click();
            driver.WaitFor(2);
            eventPage.ConfirmSaveEventChangesButton.Click();
            driver.WaitFor(2);
            driver.Navigate().Refresh();
            driver.WaitFor(5);
            eventPage.CloseEventCreationWindow.Click();
        }

        [When(@"I created a new (.*) event by filling in all the data")]
        public void WhenICreatedANewТестоваПодіяEventByFillingInAllTheData(string eventName)
        {

            var eventUniqueName = $"{eventName} {DateTime.Now.ToLocalTime()} {Guid.NewGuid().ToString("N")}";
            context.Set(eventUniqueName, eventName);
            eventsSection.CreateEventButton.Click();
            eventPage.EventTitle.SendKeys(eventUniqueName);
            eventPage.DescriptionField.SendKeys("Додаткові тестові дані");
            eventPage.AverageImportaceRadioButton.Click();
            eventPage.NotHappenRadioButton.Click();
            eventPage.FlawRadioButton.Click();
            eventPage.SecurityClassificationField.SendKeys("НВ");
            eventPage.SecurityClassificationField.SendKeys(Keys.Down);
            eventPage.SecurityClassificationField.SendKeys(Keys.Enter);
            eventPage.CountrySelectionDropDown.Click();
            var countryInput = eventPage.CountrySelectionDropDown.FindElement(By.TagName("input"));
            countryInput.SendKeys("Росія");
            driver.WaitFor(2);
            countryInput.SendKeys(Keys.Down);
            countryInput.SendKeys(Keys.Enter);
            eventPage.EventTypeDropDown.Click();
            driver.WaitFor(2);
            eventPage.EventTypeDropDown.SendKeys(Keys.Up);
            eventPage.EventTypeDropDown.SendKeys(Keys.Enter);
            eventPage.EventComponentDropDown.SendKeys("Кризові регіони");
            eventPage.EventComponentDropDown.SendKeys(Keys.Down);
            eventPage.EventComponentDropDown.SendKeys(Keys.Enter);
            eventPage.SaveEventChangesButton.Click();
            driver.WaitFor(2);
            eventPage.ConfirmSaveEventChangesButton.Click();
            driver.WaitFor(2);
            driver.Navigate().Refresh();
            eventPage.CloseEventCreationWindow.Click();
        }


        [When(@"I searched for the (.*) created event")]
        public void WhenISearchedForTheCreatedEvent(string eventName)
        {
            eventsSection.SearchButton.Click();
            var eventUniqueName = context.Get<string>(eventName);
            eventsSection.SearchField.SendKeys($"\"{eventUniqueName}\"");
            eventsSection.SearchField.SendKeys(Keys.Enter);
            driver.WaitFor(2);
        }

        [When(@"I pressed the review event button")]
        public void WhenIPressedTheReviewEventButton()
        {
            eventsSection.ReviewEventButton.Click();
            driver.WaitFor(1);
        }


        [When(@"I pressed the edit event button")]
        public void WhenIPressedTheEditEventButton()
        {
            eventsSection.EditButton.Click();
            driver.WaitFor(3);
        }

        [When(@"I binded a (.*) material to the event")]
        public void WhenIBindedAMaterialToTheEvent(string materialName)
        {
            var materialInput = eventPage.BindedMaterialsField.FindElement(By.TagName("input"));
            materialInput.SendKeys(materialName);
            driver.WaitFor(2);
            materialInput.SendKeys(Keys.Down);
            materialInput.SendKeys(Keys.Enter);
        }

        [When(@"I binded an (.*) object of study to the event")]
        public void WhenIBindedAnObjectOfStudyToTheEvent(string objectOfStudy)
        {
            var objectOfStudyInput = eventPage.BindedObjectsOfStudyField.FindElement(By.TagName("input"));
            objectOfStudyInput.SendKeys(objectOfStudy);
            driver.WaitFor(4);
            objectOfStudyInput.SendKeys(Keys.Down);
            objectOfStudyInput.SendKeys(Keys.Enter);
            driver.WaitFor(2);
        }

        [When(@"I pressed the save event changes button")]
        public void WhenIPressedTheSaveButton()
        {
            eventPage.SaveEventChangesButton.Click();
            driver.WaitFor(2);
        }

        [When(@"I pressed the confirm save changes in the event")]
        public void WhenIPressedTheConfirmSaveChangesInTheEvent()
        {
            eventPage.ConfirmSaveEventChangesButton.Click();
            driver.WaitFor(2);
        }

        [When(@"I reloaded the event page")]
        public void WhenIReloadedTheEventPage()
        {
            driver.Navigate().Refresh();
            driver.WaitFor(5);
        }

        [When(@"I clicked on the (.*) binded object of study in the event")]
        public void WhenIClickedOnTheBindedObjectOfStudyInTheEvent(string objectOfStudy)
        {
            eventPage.GetRelatedObjectOfStudyNameBindedToTheEvent(objectOfStudy).Click();
        }

        [When(@"I pressed the delete button to delete the specified (.*) material")]
        public void WhenIPressedTheDeleteButtonToDeleteTheSpecifiedMaterial(string materialName)
        {
            eventPage.MaterialsRelatedToEvent.FirstOrDefault(relatedMaterial => relatedMaterial.Title == materialName).DeleteRelation();
        }

        [When(@"I entered (.*) text in the addition data text field")]
        public void WhenIEnteredTextInTheAdditionDataTextField(string additionalText)
        {
            context.Set(additionalText, "additionalText");
            eventPage.AdditionalDataTextField.SendKeys(additionalText);
            driver.WaitFor(0.5);
        }
        [When(@"I clicked on the Events section")]
        public void WhenIClickedOnTheEventsSection()
        {
            navigationSection.EventsLink.Click();
            driver.WaitFor(5);
        }

        #endregion

        #region Then
        [Then(@"I must see the Events page")]
        public void ThenIMustSeeEventCreationButton()
        {
            Assert.Contains("events/?sort=updatedAt_desc&page=1", driver.Url);
        }

        [Then(@"I must see first event in the events list")]
        public void ThenIMustSeeFirstEventInTheEventsList()
        {
            Assert.True(eventsSection.FirstEventInTheEventsList.Displayed);
        }

        [Then(@"I must see the (.*) material binded to the event")]
        public void ThenIMustSeeTheMaterialBindedToTheEvent(string materialName)
        {
            Assert.True(eventPage.IsMaterialVisible(materialName));
        }

        [Then(@"I must see the (.*) event in the event search results")]
        public void ThenIMustSeeTheEvent(string eventName)
        {
            var eventUniqueName = context.Get<string>(eventName);
            Assert.True(eventPage.IsEventVisible(eventUniqueName));
        }
        [Then(@"I must not see the (.*) material binded to the event")]
        public void ThenIMustNotSeeTheMaterialBindedToTheEvent(string materialName)
        {
            Assert.DoesNotContain(eventPage.MaterialsRelatedToEvent, _ => _.Title == materialName);
        }

        [Then(@"I must see the (.*) text in the additional data text field")]
        public void ThenIMustSeeTheAdditionalDataInTheAdditionalTextField(string text)
        {
            var additionalText = context.Get<string>("additionalText");
            var actualText = eventPage.AdditionalDataTextField.GetAttribute("value");
            Assert.Contains(additionalText, actualText);
        }

        [Then(@"I open first event in the events list")]
        public void ThenIOpenFirstEventInTheEventsList()
        {
            var viewEventButton = eventsSection.FirstEventInTheEventsListFullscreenButton;
            viewEventButton.Click();
        }

        #endregion
    }
}