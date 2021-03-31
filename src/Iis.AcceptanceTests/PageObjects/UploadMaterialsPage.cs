using OpenQA.Selenium;
using SeleniumExtras.PageObjects;

namespace AcceptanceTests.PageObjects
{
    public class UploadMaterialsPageObjects
    {
        private readonly IWebDriver driver;

        public UploadMaterialsPageObjects(IWebDriver driver)
        {
            this.driver = driver;
            PageFactory.InitElements(driver, this);
        }

        [FindsBy(How = How.CssSelector, Using = "li:nth-of-type(6) > .sidebar__nav-item-title")]
        [CacheLookup]
        public IWebElement LoadMaterialsSection;

        [FindsBy(How = How.XPath, Using = "//div[@class='upload__form-controls']//span[contains(text(),'Завантажити')]")]
        [CacheLookup]
        public IWebElement UploadMaterialButton;
    }
}