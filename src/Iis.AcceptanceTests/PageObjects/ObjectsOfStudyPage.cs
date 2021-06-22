using System.Collections.Generic;
using System.Linq;
using AcceptanceTests.PageObjects;
using Iis.AcceptanceTests.PageObjects.Controls;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using SeleniumExtras.PageObjects;

namespace AcceptanceTests.PageObjects
{
    public class ObjectsOfStudyPageObjects
    {
        public IWebDriver driver;

        public ObjectsOfStudyPageObjects(IWebDriver driver)
        {
            this.driver = driver;
            PageFactory.InitElements(driver, this);
        }

        [FindsBy(How = How.XPath, Using = "//div[contains(text(),'Обʼєкти')]")]
        public IWebElement ObjectOfStudySectionButton;

        [FindsBy(How = How.CssSelector, Using = ".entity-search .entity-search__toggle button")]
        [CacheLookup]
        public IWebElement SearchLoopButton;

        [FindsBy(How = How.CssSelector, Using = "span > .el-input > .el-input__inner")]
        [CacheLookup]
        public IWebElement SearchField;

        [FindsBy(How = How.XPath, Using = "//div[@class='summary-person-row text-ellipsis']/span[@class='title']")]
        public IWebElement FirstSearchResultTitle;

        [FindsBy(How = How.XPath, Using = "//div[@class='infinity-table objects-table']//tbody[@class='p-datatable-tbody']/tr[1]")]
        public IWebElement FirstSearchResultRow;

        [FindsBy(How = How.XPath, Using = "//div[@class='icon-wrapper icon-wrapper-edit']")]
        [CacheLookup]
        public IWebElement EditObjectOfStudyButton;

        [FindsBy(How = How.XPath, Using = "//div[contains(text(),'Класифікатори')]")]
        public IWebElement ClassifierBlock;

        [FindsBy(How = How.CssSelector, Using = "textarea[name='title']")]
        public IWebElement RealNameFullField;

        [FindsBy(How = How.XPath, Using = "//div[contains(text(),'Загальна інформація')]")]
        [CacheLookup]
        public IWebElement GeneralInfoBlock;

        [FindsBy(How = How.XPath, Using = "//div[@class='readonly tag link']")]
        [CacheLookup]
        public IWebElement DirectReportingRelationshipLink;

        [FindsBy(How = How.XPath, Using = "(//div[@class='text-ellipsis title'])[1]")]
        public IWebElement TitleOfTheFirstObject;

        [FindsBy(How = How.CssSelector, Using = ".summary-person-row .title")]
        [CacheLookup]
        public IWebElement PersonSearchResult;

        [FindsBy(How = How.CssSelector, Using = ".p-datatable-scrollable-body-table tr:nth-child(1) .highlight span em")]
        [CacheLookup]
        public IWebElement FirstSearchResultSignValue;

        [FindsBy(How = How.CssSelector, Using = ".entity-search__result-counter")]
        [CacheLookup]
        public IWebElement SearchCounterInOOSSearchField;

        [FindsBy(How = How.CssSelector, Using = "tbody > tr:nth-of-type(1)")]
        [CacheLookup]
        public IWebElement FirstElementInTheOOSList;

        [FindsBy(How = How.CssSelector, Using = "tbody > tr:nth-of-type(1)")]
        [CacheLookup]
        public IWebElement ObjectOfStudySmallCardWindow;

        [FindsBy(How = How.CssSelector, Using = ".text-ellipsis.is-align-middle")]
        [CacheLookup]
        public IWebElement ObjectTitleInTheSmallCard;

        [FindsBy(How = How.XPath, Using = "//span[text()=' Зберегти тему ']")]
        public IWebElement CreateThemeButton;

        [FindsBy(How = How.CssSelector, Using = "button[name='btn-save']")]
        public IWebElement SaveObjectOfStudyButton;

        [FindsBy(How = How.CssSelector, Using = "button[name='btn-full-screen']")]
        public IWebElement EnlargeObjectOfStudySmallCardButton;

        [FindsBy(How = How.XPath, Using = "//button[@name='btn-collapse']")]
        public IWebElement MinimizeObjectOfStudyBigCardButton;

        [FindsBy(How = How.CssSelector, Using = ".el-menu > [role='menuitem']:nth-of-type(1)")]
        [CacheLookup]
        public IWebElement BigCardProfileTab;

        [FindsBy(How = How.CssSelector, Using = ".el-menu > .el-menu-item:nth-of-type(2)")]
        [CacheLookup]
        public IWebElement BigCardMaterialsTab;

        [FindsBy(How = How.CssSelector, Using = ".el-menu > .el-menu-item:nth-of-type(3)")]
        [CacheLookup]
        public IWebElement BigCardEventsTab;

        [FindsBy(How = How.CssSelector, Using = "ul[role='menubar'] > li:nth-of-type(4)")]
        [CacheLookup]
        public IWebElement BigCardChangeHistoryTab;

        [FindsBy(How = How.CssSelector, Using = ".el-menu > .el-menu-item:nth-of-type(5)")]
        [CacheLookup]
        public IWebElement BigCardMapTab;

        [FindsBy(How = How.CssSelector, Using = "ul[role='menubar'] > li:nth-of-type(6)")]
        [CacheLookup]
        public IWebElement BigCardRelationsTab;

        [FindsBy(How = How.CssSelector, Using = "div[name='affiliation'] div[name='view-item-relation']")]
        [CacheLookup]
        public IWebElement BigCardAffiliation;

        [FindsBy(How = How.CssSelector, Using = "div[name='importance'] div[name='view-item-relation']")]
        [CacheLookup]
        public IWebElement BigCardImportance;

        [FindsBy(How = How.CssSelector, Using = ".entity-search__result-counter")]
        public IWebElement OOSSearchCounter;

        [FindsBy(How = How.CssSelector, Using = ".objects-table .p-datatable-emptymessage .empty-state__message")]
        [CacheLookup]
        public IWebElement OOSEmptySearchResults;

        [FindsBy(How = How.XPath, Using = "//h3[@class='title']")]
        [CacheLookup]
        public IWebElement OOSTitle;

        [FindsBy(How = How.CssSelector, Using = "ul[role='menubar'] > li:nth-of-type(2)")]
        [CacheLookup]
        public IWebElement HierarchyTab;

        [FindsBy(How = How.CssSelector, Using = "div:nth-child(1) > div:nth-child(4) > div:nth-of-type(1) .hierarchy-card__expand")]
        [CacheLookup]
        public IWebElement RussianMilitaryForcesExpandButton;

        [FindsBy(How = How.XPath, Using = "//ul//li[contains(.,\"Зв'язки\")]")]
        public IWebElement RelationsTab;

        [FindsBy(How = How.XPath, Using = "//div[contains(text(),'3 окрема мотострілецька бригада \"Беркут\"')]")]
        [CacheLookup]
        public IWebElement ThirdBrigadeSearchResult;

        [FindsBy(How = How.XPath, Using = "//span[contains(text(),' Створити обʼєкт... ')]")]
        public IWebElement CreateANewObjectOfStudyButton;

        [FindsBy(How = How.XPath, Using = "//span[contains(@class, 'el-tree-node__label') and text() = 'Військовий підрозділ']")]
        public IWebElement CreateAMilitaryOrganizationButton;

        [FindsBy(How = How.CssSelector, Using = "div[name='affiliation'] .el-input__inner")]
        public IWebElement AffiliationField;

        [FindsBy(How = How.CssSelector, Using = "div[name='importance'] .el-input__inner")]
        public IWebElement ImportanceField;

        [FindsBy(How = How.XPath, Using = "//div[@name='accessLevel']//input[@type='text']")]
        public IWebElement SecurityClassificationField;

        [FindsBy(How = How.CssSelector, Using = "//div[contains(text(),' Класифікатори ')]")]
        public IWebElement ClassifiersBlock;

        [FindsBy(How = How.CssSelector, Using = "div[name='parent'] .el-input__inner")]
        public IWebElement DirectReportingRelationship;

        [FindsBy(How = How.CssSelector, Using = ".confirm-message-box button.confirm-message-box__action-confirm")]
        public IWebElement ConfirmSaveOfANewObjectOfStudyButton;

        [FindsBy(How = How.XPath, Using = "//div[contains(text(),' Дислокація ')]")]
        public IWebElement DislocationBlock;

        [FindsBy(How = How.XPath, Using = "//div[contains(text(),' Тимчасова дислокація ')]")]
        public IWebElement TemporaryDislocationBlock;

        [FindsBy(How = How.CssSelector, Using = "div[name = 'country'] .el-input__inner")]
        public IWebElement CountryFieldInTheDisclocationBlock;

        [FindsBy(How = How.CssSelector, Using = ".el-input--mini:nth-of-type(1) .el-input__inner")]
        public IWebElement LongitudeField;

        [FindsBy(How = How.CssSelector, Using = ".el-input--mini:nth-of-type(2) .el-input__inner")]
        public IWebElement LatitudeField;

        [FindsBy(How = How.CssSelector, Using = ".el-collapse-item:nth-of-type(22) .el-input--mini:nth-of-type(1) .el-input__inner")]
        public IWebElement TemporaryDislocationLatitudeField;

        [FindsBy(How = How.CssSelector, Using = ".el-collapse-item:nth-of-type(22) .el-input--mini:nth-of-type(2) .el-input__inner")]
        public IWebElement TemporaryDislocationLongitudeField;

        [FindsBy(How = How.CssSelector, Using = ".el-collapse-item:nth-of-type(22) .el-input--mini:nth-of-type(2) .el-input__inner")]
        public IWebElement CountryFieldInTheTemporaryDisclocationBlock;

        [FindsBy(How = How.XPath, Using = "//li[contains(text(),'Події')]")]
        public IWebElement EventsTabInTheBigObjectOfStudyCard;

        [FindsBy(How = How.XPath, Using = "//div[@name='title']/div[@class='el-form-item__content']")]
        public IWebElement RealNameFullBlock;


        [FindsBy(How = How.CssSelector, Using = "div[name = 'country'] .el-input__inner")]
        public IWebElement DislocationSectionCountryField;

        [FindsBy(How = How.XPath, Using = "//span[@class='multiselect__tag-label']")]
        public IWebElement TagInTheSearchField;

        public ObjectsSearch GetObjectByTitle(string title)
        {
            return new ObjectsSearch(driver, title);
        }
        public HierarchyCard GetHierarchyCardByTitle(string title)
        {
            return new HierarchyCard(driver, title);
        }

        public List<Event> Events => driver.FindElements(By.CssSelector(".events-table tbody.p-datatable-tbody > tr"))
                   .Select(webElement => new Event(driver, webElement)).ToList();

        public void ScrollDown(string value)
        {
            var elementToScroll = driver.FindElement(By.XPath($"//div[contains(text(),'{value}')]"));
            Actions actions = new Actions(driver);
            actions.MoveToElement(elementToScroll).Perform();
        }

        public void ClickOnTheDirectReportingRelationshipLink(string relationshipLinkTitle)
        {
            var elementToClick = driver.FindElement(By.XPath($"//div[contains(text(),'{relationshipLinkTitle}')]"));
            elementToClick.Click();
        }

    }
}