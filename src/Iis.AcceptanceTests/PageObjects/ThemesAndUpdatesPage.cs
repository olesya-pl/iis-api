using OpenQA.Selenium;
using SeleniumExtras.PageObjects;

namespace Iis.AcceptanceTests.PageObjects
{
    public class ThemesAndUpdatesPageObjects
    {
        public IWebDriver driver;

        public ThemesAndUpdatesPageObjects(IWebDriver driver)
        {
            this.driver = driver;
            PageFactory.InitElements(driver, this);
        }

        [FindsBy(How = How.CssSelector, Using = ".sidebar__body .sidebar__nav:nth-of-type(1) [role='menuitem']:nth-of-type(4)")]
        [CacheLookup]
        public IWebElement ThemesAndUpdatesSection;

        [FindsBy(How = How.CssSelector, Using = "tbody .el-table__row:nth-of-type(1)")]
        [CacheLookup]
        public IWebElement FirstThemeInTheThemeList;

    }
}