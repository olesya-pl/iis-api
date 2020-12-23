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

        [FindsBy(How = How.CssSelector, Using = ".sidebar .sidebar__nav-item[name='themes']")]
        [CacheLookup]
        public IWebElement ThemesAndUpdatesSection;

        [FindsBy(How = How.CssSelector, Using = ".themes-table .theme:nth-of-type(1)")]
        [CacheLookup]
        public IWebElement FirstThemeInTheThemeList;

        [FindsBy(How = How.XPath, Using = "//input[@placeholder='Введіть назву теми']")]
        public IWebElement EnterThemeNameField;
        public List<Theme> Themes => driver.FindElements(By.CssSelector(".themes-table .theme"))
                    .Select(_ => new Theme(driver, _)).ToList();
        public ThemeSideView SideView => new ThemeSideView(driver);
        public Theme GetThemeByTitle(string title)
        {
            return new Theme(driver, title);
        }
    }
}