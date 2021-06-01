using System.Collections.Generic;
using System.Linq;
using AcceptanceTests.PageObjects.Controls;
using OpenQA.Selenium;
using SeleniumExtras.PageObjects;

namespace AcceptanceTests.PageObjects
{
    public class ThemesAndUpdatesPageObjects
    {
        public IWebDriver driver;

        public ThemesAndUpdatesPageObjects(IWebDriver driver)
        {
            this.driver = driver;
            PageFactory.InitElements(driver, this);
        }

        [FindsBy(How = How.XPath, Using = "//div[@class='sidebar__body']//li[@class='el-menu-item sidebar__nav-item themes']")]
        [CacheLookup]
        public IWebElement ThemesAndUpdatesSection;

        [FindsBy(How = How.CssSelector, Using = ".themes-table .p-datatable-tbody > tr")]
        [CacheLookup]
        public IWebElement FirstThemeInTheThemeList;

        [FindsBy(How = How.XPath, Using = "//input[@placeholder='Введіть назву теми']")]
        public IWebElement EnterThemeNameField;
        public List<Theme> Themes => driver.FindElements(By.CssSelector(".themes-table tr"))
                    .Select(_ => new Theme(driver, _)).ToList();
        public ThemeSideView SideView => new ThemeSideView(driver);
        public Theme GetThemeByTitle(string title)
        {
            return new Theme(driver, title);
        }
    }
}