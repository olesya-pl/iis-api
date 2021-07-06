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

        [FindsBy(How = How.XPath, Using = "//div[@class='upload']")]
        public IWebElement UploadFunctionalityWindow;
    }
}