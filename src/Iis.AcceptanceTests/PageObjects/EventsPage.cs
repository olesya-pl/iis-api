using System.Collections.Generic;
using System.Linq;
using Iis.AcceptanceTests.PageObjects.Controls;
using OpenQA.Selenium;
using SeleniumExtras.PageObjects;

namespace AcceptanceTests.PageObjects
{

    public class EventsPageObjects
    {
        private readonly IWebDriver driver;

        public EventsPageObjects(IWebDriver driver)
        {

            this.driver = driver;

            PageFactory.InitElements(driver, this);
        }

        [FindsBy(How = How.XPath, Using = "//li[@name='events']")]
        public IWebElement EventsPage;

        [FindsBy(How = How.CssSelector, Using = ".add-button")]
        [CacheLookup]
        public IWebElement CreateEventButton;

        [FindsBy(How = How.CssSelector, Using = "tbody > tr:nth-of-type(1)")]
        [CacheLookup]
        public IWebElement FirstEventInTheEventsList;

        [FindsBy(How = How.XPath, Using = "//textarea[@name='name']")]
        public IWebElement EventTitle;

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

        [FindsBy(How = How.CssSelector, Using = ".el-tooltip")]
        public IWebElement SearchButton;

        [FindsBy(How = How.CssSelector, Using = ".action-button--edit")]
        public IWebElement EditButton;

        [FindsBy(How = How.CssSelector, Using = "button[name='btn-close'] > .el-icon-close")]
        public IWebElement CloseEventCreationWindow;

        [FindsBy(How = How.CssSelector, Using = "[aria-describedby] .el-input__inner")]
        public IWebElement SearchField;

        public List<Event> Events => driver.FindElements(By.ClassName("el-table__row"))
                    .Select(webElement => new Event(driver, webElement)).ToList();

        public List<Event> GetEventsByName(string eventName)
        {
            return Events.Where(i => i.Name.Contains(eventName)).ToList();
        }
        public Event GetEventByTitle(string title)
        {
            return new Event(driver, title);
        }
    }
}
