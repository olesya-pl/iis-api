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

        [FindsBy(How = How.CssSelector, Using = ".themes-table .p-datatable-tbody > tr")]
        [CacheLookup]
        public IWebElement FirstThemeInTheThemeList;

        [FindsBy(How = How.CssSelector, Using = ".create-entity-theme__form .create-entity-theme__form-field input[type=text]")]
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