using OpenQA.Selenium;
using SeleniumExtras.PageObjects;

namespace AcceptanceTests.PageObjects
{
    public class AdministrationPageObjects
    {
        private readonly IWebDriver driver;

        public AdministrationPageObjects(IWebDriver driver)
        {
            this.driver = driver;

            PageFactory.InitElements(driver, this);
        }

        [FindsBy(How = How.XPath, Using = "//div[contains(text(),'Адміністрування')]")]
        public IWebElement AdministrationPage;

        [FindsBy(How = How.CssSelector, Using = "tbody > tr:nth-of-type(1)")]
        public IWebElement FirstUserOnTheAdminPage;


    }
}
