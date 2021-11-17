using System.Collections.Generic;
using System.Linq;
using Iis.AcceptanceTests.PageObjects.Controls;
using OpenQA.Selenium;
using SeleniumExtras.PageObjects;
using Xunit;

namespace AcceptanceTests.PageObjects
{

    public class EventPage
    {
        private readonly IWebDriver driver;

        public EventPage(IWebDriver driver)
        {

            this.driver = driver;

            PageFactory.InitElements(driver, this);
        }

        [FindsBy(How = How.XPath, Using = "//textarea[@name='name']")]
        public IWebElement EventTitle;

        [FindsBy(How = How.CssSelector, Using = " .aside-card .event-card .event-card__description")]
        public IWebElement DescriptionField;

        [FindsBy(How = How.CssSelector, Using = ".normal .el-radio__inner")]
        public IWebElement AverageImportaceRadioButton;

        [FindsBy(How = How.CssSelector, Using = ".notHappen .el-radio__inner")]
        public IWebElement NotHappenRadioButton;

        [FindsBy(How = How.XPath, Using = "//div[@name='dateInterval']/div/div/input[1]")]
        public IWebElement StartEventDateField;

        [FindsBy(How = How.CssSelector, Using = "div[name='state']  div[role='radiogroup'] > label:nth-of-type(1)  .el-radio__inner")]
        public IWebElement FlawRadioButton;

        [FindsBy(How = How.XPath, Using = "//div[@name='accessLevel']//input[@type='text']")]
        public IWebElement SecurityClassificationField;

        [FindsBy(How = How.CssSelector, Using = "div[name='relatesToCountry'] > .el-form-item__content")]
        public IWebElement CountrySelectionDropDown;

        [FindsBy(How = How.CssSelector, Using = "div[name='eventType'] .el-input__inner")]
        public IWebElement EventTypeDropDown;

        [FindsBy(How = How.CssSelector, Using = "div[name='component'] .el-input__inner")]
        public IWebElement EventComponentDropDown;

        [FindsBy(How = How.CssSelector, Using = "button[name='btn-save'] > span")]
        public IWebElement SaveEventChangesButton;

        [FindsBy(How = How.XPath, Using = "//span[contains(text(),'Підтвердити')]")]
        public IWebElement ConfirmSaveEventChangesButton;

        [FindsBy(How = How.CssSelector, Using = "button[name='btn-close'] > .el-icon-close")]
        public IWebElement CloseEventCreationWindow;

        [FindsBy(How = How.XPath, Using = "//div[@class='event-card__linked-materials']")]
        public IWebElement BindedMaterialsField;

        [FindsBy(How = How.XPath, Using = "//div[@class='el-form-item event-card__objects-associated-with-event el-form-item--small']")]
        public IWebElement BindedObjectsOfStudyField;

        [FindsBy(How = How.CssSelector, Using = "textarea[name='description']")]
        public IWebElement AdditionalDataTextField;

        public Event GetRelatedObjectOfStudyNameBindedToTheEvent(string title)
        {
            return new Event(driver, title);
        }

        public List<Event> Events => driver.FindElements(By.CssSelector(".events-table .p-datatable-tbody > tr"))
                    .Select(webElement => new Event(driver, webElement)).ToList();
        public List<EventRelatedItems> MaterialsRelatedToEvent => driver.FindElement(By.CssSelector(".event-card__linked-materials")).FindElements(By.CssSelector(".el-tag"))
                    .Select(webElement => new EventRelatedItems(driver, webElement)).ToList();

        public Event GetEventByTitle(string title)
        {
            return new Event(driver, title);
        }

        public bool IsMaterialVisible(string materialName)
        {
            var bindedMaterial = driver.FindElement(By.XPath($"//*[contains(@class, 'event-card__linked-materials')]//*[contains(@class, 'wrapper')]//*[text()]"));
            return bindedMaterial.Displayed;
        }

        public bool IsEventVisible(string eventName)
        {
            var eventInTheList = driver.FindElement(By.XPath($"//div[contains(text(),'{eventName}')]"));
            return eventInTheList.Displayed;
        }
    }
}
