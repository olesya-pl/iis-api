using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using SeleniumExtras.PageObjects;
using System.Collections.Generic;
using System.Linq;

namespace AcceptanceTests.PageObjects
{
    public class MaterialsSectionPage
    {
        private readonly IWebDriver driver;

        public MaterialsSectionPage(IWebDriver driver)
        {

            this.driver = driver;

            PageFactory.InitElements(driver, this);
        }

        [FindsBy(How = How.XPath, Using = "//div[@class='infinity-table materials-table']//tbody[@class='p-datatable-tbody']/tr")]
        public IWebElement FirstMaterialInTheMaterialsList;

        [FindsBy(How = How.CssSelector, Using = ".el-button--default")]
        public IWebElement SearchButton;

        [FindsBy(How = How.XPath, Using = ".//*[text()='Оброблено']")]   //".material-status .is-processed")]
        public IWebElement ProcessedStatusHightlight;


        [FindsBy(How = How.CssSelector, Using = ".el-input.entity-search__input > .el-input__inner")]
        public IWebElement SearchField;

        [FindsBy(How = How.CssSelector, Using = ".materials-table .p-datatable-emptymessage .empty-state__message")]
        [CacheLookup]
        public IWebElement EmptySearchField;

        [FindsBy(How = How.CssSelector, Using = ".el-button--success")]
        public IWebElement ProcessedButton;

        [FindsBy(How = How.CssSelector, Using = "div:nth-of-type(1) > .meta-data-card > .meta-data-expand  .el-button.el-button--default > span")]
        [CacheLookup]
        public IWebElement ShowMLResultsButton;

        [FindsBy(How = How.CssSelector, Using = ".action-tab--connection")]
        public IWebElement RelationsTab;

        [FindsBy(How = How.CssSelector, Using = ".action-tab--features")]
        public IWebElement PatternTab;

        [FindsBy(How = How.XPath, Using = "//span[contains(text(),'value')]/following-sibling::span[1]")]
        public IWebElement PhoneNumberPatternNode;

        [FindsBy(How = How.CssSelector, Using = ".material__tabs-menu > .action-tab--ml")]
        public IWebElement MLTab;

        [FindsBy(How = How.CssSelector, Using = ".material-events__header .el-input__inner")]
        [CacheLookup]
        public IWebElement EventsSearch;

        [FindsBy(How = How.CssSelector, Using = ".material-objects input")]
        public IWebElement ObjectsSearch;

        [FindsBy(How = How.CssSelector, Using = ".sidebar__nav li.objects")]
        public IWebElement ObjectsTabSearch;

        public MaterialPage MaterialPage => new MaterialPage(driver);

        [FindsBy(How = How.CssSelector, Using = "div:nth-of-type(2) > .el-form-item__content > .el-select.el-tooltip")]
        [CacheLookup]
        public IWebElement AuthenticityDropDown;

        [FindsBy(How = How.CssSelector, Using = ".material-general .material-intelligence .material-group-info__row")]
        [CacheLookup]
        public IWebElement RelevanceDropDown;

        [FindsBy(How = How.CssSelector, Using = ".material-general .material-intelligence .action-select--importance")]
        [CacheLookup]
        public IWebElement ImportanceDropDown;

        [FindsBy(How = How.CssSelector, Using = ".material-general .material-intelligence .action-select--completeness")]
        [CacheLookup]
        public IWebElement СompletenessOfInformation;

        [FindsBy(How = How.CssSelector, Using = ".material-general .material-intelligence .action-select--reliability")]
        [CacheLookup]
        public IWebElement SourceCredibility;

        [FindsBy(How = How.CssSelector, Using = ".material-general .material-assignee .action-select--assignee")]
        [CacheLookup]
        public IWebElement Originator;

        [FindsBy(How = How.CssSelector, Using = ".entity-search__result-counter")]
        public IWebElement MaterialsSearchResultCounter;

        [FindsBy(How = How.CssSelector, Using = ".action-button--prev-page span")]
        public IWebElement PreviousMaterialButton;

        [FindsBy(How = How.CssSelector, Using = ".materials-table tbody.p-datatable-tbody > tr")]
        public IWebElement FirstSearchResult;

        [FindsBy(How = How.CssSelector, Using = ".meta-data__list .meta-data__list-item:nth-of-type(3) .el-button--default")]
        [CacheLookup]
        public IWebElement TextClassifierMLOutputButton;

        [FindsBy(How = How.CssSelector, Using = ".meta-data__list .meta-data__list-item:nth-of-type(3) .meta-data-card__result-body")]
        [CacheLookup]
        public IWebElement TextClassifierMLOutputForm;

        [FindsBy(How = How.CssSelector, Using = ".is-scrolling-none")]
        [CacheLookup]
        public IWebElement EmptyAreInTheMaterialList;

        [FindsBy(How = How.CssSelector, Using = ".material-objects .material-objects-table a")]
        public IWebElement BindedObjectLink;

        [FindsBy(How = How.CssSelector, Using = ".confirm-message-box__action-confirm")]
        public IWebElement ConfirmDeleteRelationButton;

        [FindsBy(How = How.CssSelector, Using = "tbody.p-datatable-tbody .materials-table__title")]
        [CacheLookup]
        public IWebElement FirstSearchResultContentBlock;

        [FindsBy(How = How.XPath, Using = "//button[@name='delete']")]
        [CacheLookup]
        public IWebElement DeleteRelatedObjectOfStudy;

        [FindsBy(How = How.XPath, Using = "//button[@name='delete']")]
        public IWebElement DeleteRelatedEventButton;

        [FindsBy(How = How.XPath, Using = "//td[@class='materials-table__title']//div")]
        public IWebElement MaterialTitle;

        [FindsBy(How = How.XPath, Using = "//button[contains(@class, 'search__clear-button')]")]
        public IWebElement ClearSearchFieldButton;

        [FindsBy(How = How.CssSelector, Using = ".icon-wrapper-close")]
        public IWebElement CloseMaterialCardButton;

        [FindsBy(How = How.CssSelector, Using = "button.action-button--important")]
        public IWebElement MaterialImportantButton;

        [FindsBy(How = How.CssSelector, Using = "button.action-button--immediateReport")]
        public IWebElement MaterialImmediateReportButton;

        [FindsBy(How = How.CssSelector, Using = "button.action-button--translation")]
        public IWebElement MaterialTranslationButton;

        [FindsBy(How = How.CssSelector, Using = ".p-datatable-scrollable-header-table .materials-table__source .p-sortable-column-icon")]
        public IWebElement SourceSortable;

        [FindsBy(How = How.CssSelector, Using = ".material-general-access-level .material-info-card__body .el-input__inner")]
        public IWebElement AccessLevelField;

        [FindsBy(How = How.XPath, Using = "//div/table/thead/tr/th[@class='materials-table__source p-sortable-column'][@aria-sort='none']")]
        public IWebElement SortedBySourceSortingNull;

        [FindsBy(How = How.CssSelector, Using = ".material-relations-input .el-input__inner")]
        public IWebElement ObjectsSearchTextBox;

        [FindsBy(How = How.CssSelector, Using = "button[class='el-button el-button--primary'] > span")]
        public IWebElement SaveChangesButton;

        [FindsBy(How = How.XPath, Using = "//tr[1]//div[@class='input-stream-related-objects']//div/a")]
        public IWebElement ObjectOfStudyContainer;

        [FindsBy(How = How.XPath, Using = "//div[@class='material__viewer']//div[@class='wavesurfer__instance-wrapper']//div[@class='wavesurfer__timeline']")]
        public IWebElement ClickedOnTheHotKey;

        [FindsBy(How = How.XPath, Using = "//*[contains(@class, 'toastui-editor-main toastui-editor-ww-mode')]//*[contains(@class, 'toastui-editor-ww-container')]//*[contains(@class, 'ProseMirror toastui-editor-contents')]")]
        public IWebElement TextField;

        public void ScrollToEnd()
        {
            Actions actions = new Actions(driver);
            actions.SendKeys(Keys.Control).SendKeys(Keys.End).Perform();
        }

        public List<MaterialRelatedItems> MaterialsRelatedEvents => driver.FindElements(By.CssSelector(".material-events-table"))
                   .Select(_ => new MaterialRelatedItems(driver, _)).ToList();

        public List<MaterialRelatedItems> MaterialsRelatedObjects => driver.FindElements(By.CssSelector(".material-objects-table"))
                  .Select(_ => new MaterialRelatedItems(driver, _)).ToList();

        public MaterialRelatedItems GetItemTitleRelatedToMaterial(string title)
        {
            return new MaterialRelatedItems(driver, title);
        }

        public bool SortedMaterialsBySource(string sourceName)
        {
            var SourceName = driver.FindElement(By.XPath($"//tr[1]/*[contains(@class,'materials-table__source')]//*[contains(text(),'{sourceName}')]"));
            return SourceName.Displayed;
        }

        public void TypesFilter(string checkboxLabel)
        {
            var CheckboxLabel = driver.FindElement(By.XPath($"//*[contains(@class, 'materials-table')]//*[contains(@class, 'table-zones__aside-body')]//*[contains(@class, 'aggregation-group')]//*[contains(text(),'{checkboxLabel}')]"));
            Actions actions = new Actions(driver);
            actions.Click(CheckboxLabel).Perform();
        }
 
        [FindsBy(How = How.XPath, Using = "//div[@class='wavesurfer__timeline']//*[contains(@style, 'position: absolute; z-index: 3; left: 0px; top: 0px; bottom: 0px; overflow: hidden; width: 0px; display: block; box-sizing: border-box; border-right: 1px solid rgb(51, 51, 51); pointer-events: none;')]")]
        public IWebElement PositionOfTimeline;

        public void ClickPauseButton()
        {
            var PauseButton = driver.FindElement(By.XPath($"//*[contains(@class, 'el-container material-page is-vertical')]//*[contains(@class, 'material__viewer')]//*[contains(@class, 'wavesurfer__control wavesurfer__control--play')]//*[contains(@class, 'el-tooltip wavesurfer__control-button')]"));
            Actions actions = new Actions(driver);
            actions.Click(PauseButton).Perform();
        }

        [FindsBy(How = How.XPath, Using = "//div[@class='material__viewer']//div[@class='wavesurfer__instance-wrapper']//div[@class='wave-timeline__overview']")]
        public IWebElement LengthAudioTrack;

        [FindsBy(How = How.XPath, Using = "//div[@class='p-datatable-scrollable-body']//tr[1]//div[@class='material-type material-type--hoverable']")]
        public IWebElement MaterialTypeIcon;

        [FindsBy(How = How.XPath, Using = "//div[@class='p-datatable-scrollable-body']//div[@class='audio-player wavesurfer']")]
        public IWebElement AudioPlayer;

        [FindsBy(How = How.XPath, Using = "//div[@class='p-datatable-scrollable-body']//div[@class='player']//div[@class='player__controls-panel']//*[@class='player__timer-total']")]
        public IWebElement PlayerControlsPanelWithTotalTime;

        [FindsBy(How = How.CssSelector, Using = ".action-button--important")]
        [CacheLookup]
        public IWebElement SessionPriorityTranslate;

        [FindsBy(How = How.CssSelector, Using = ".action-button--translation")]
        [CacheLookup]
        public IWebElement SessionPriorityImportant;

        [FindsBy(How = How.CssSelector, Using = ".action-button--immediateReport")]
        [CacheLookup]
        public IWebElement SessionPriorityImmediateRepor;

        [FindsBy(How = How.XPath, Using = "//*[@class='el-notification right']")]
        public IWebElement Notification;

        [FindsBy(How = How.CssSelector, Using = ".action-button--next-page span")]
        public IWebElement NextMaterialButton;

        [FindsBy(How = How.XPath, Using = "//*[@class='material-viewer__editor']//*[@class='ProseMirror-trailingBreak']")]
        public IWebElement ClearTextField;

        [FindsBy(How = How.CssSelector, Using = ".action-button--similar-search")]
        public IWebElement GetSimilarMaterials;

        [FindsBy(How = How.XPath, Using = "//div[@class='base-page__header-body']//span[@class='el-tag el-tag--danger el-tag--light']")]
        public IWebElement SimilarMaterials;
    }
}