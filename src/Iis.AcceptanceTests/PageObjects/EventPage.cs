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

        [FindsBy(How = How.XPath, Using = "//textarea[@name='description']")]
        public IWebElement DescriptionField;

        [FindsBy(How = How.CssSelector, Using = "div[name='importance']  div[role='radiogroup'] > label:nth-of-type(1)  .el-radio__inner")]
        public IWebElement AverageImportaceRadioButton;

        [FindsBy(How = How.CssSelector, Using = "div[name='state']  div[role='radiogroup'] > label:nth-of-type(1)  .el-radio__inner")]
        public IWebElement FlawRadioButton;

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

        [FindsBy(How = How.CssSelector, Using = ".event-card__linked-materials .container")]
        public IWebElement BindedMaterialsField;

        [FindsBy(How = How.CssSelector, Using = ".event-card__objects-associated-with-event .container")]
        public IWebElement BindedObjectsOfStudyField;

        public Event GetRelatedObjectOfStudyNameBindedToTheEvent(string title)
        {
            return new Event(driver, title);
        }

        //private IWebElement Container => driver.FindElement(By.CssSelector(".event-card__linked-materials .container"));

        //private IReadOnlyCollection<IWebElement> RelatedItems => Container.FindElements(By.CssSelector(".el-tag--mini"));

        public List<Event> Events => driver.FindElements(By.ClassName("el-table__row"))
                    .Select(webElement => new Event(driver, webElement)).ToList();
        public List<EventRelatedItems> MaterialsRelatedToEvent => driver.FindElement(By.CssSelector(".event-card__linked-materials .container")).FindElements(By.CssSelector(".el-tag--mini"))
                    .Select(webElement => new EventRelatedItems(driver, webElement)).ToList();

        public Event GetEventByTitle(string title)
        {
            return new Event(driver, title);
        }

        public bool IsMaterialVisible(string materialName)
        {
            var bindedMaterial = driver.FindElement(By.XPath($"//span[contains(text(),'{materialName}')]"));
            return bindedMaterial.Displayed;
        }

        public bool IsEventVisible(string eventName)
        {
            var eventInTheList = driver.FindElement(By.XPath($"//span[contains(text(),'{eventName}')]"));
            return eventInTheList.Displayed;
        }
    }
}
