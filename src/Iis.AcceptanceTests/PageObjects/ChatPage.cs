using OpenQA.Selenium;
using SeleniumExtras.PageObjects;

namespace AcceptanceTests.PageObjects
{
    public class ChatPage
    {
        private readonly IWebDriver driver;

        public ChatPage(IWebDriver driver)
        {
            this.driver = driver;
            PageFactory.InitElements(driver, this);
        }

        [FindsBy(How = How.XPath, Using = "//div[@class='chat']//div[@class='room-panel']//*[text()=' Люди ']")]
        public IWebElement ListOfUsers;
    }
}