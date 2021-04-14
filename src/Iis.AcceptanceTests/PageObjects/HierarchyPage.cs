using AcceptanceTests.Helpers;
using AcceptanceTests.PageObjects.Controls;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;

namespace AcceptanceTests.PageObjects
{
    public class HierarchyCard
    {
        private readonly IWebDriver driver;

        private readonly IWebElement card;

        private readonly string title;

        private readonly Actions actions;

        private IWebElement toggle => driver.FindElement(By.XPath($@"//div[contains(text(),""{title}"")]/following::div[1]"));

        public bool Displayed => card.Displayed;

        public HierarchyCard(IWebDriver driver, string title)
        {
            this.driver = driver;
            this.title = title;
            actions = new Actions(driver);
            card = driver.FindElement(By.XPath($@"//div[contains(text(),""{title}"")]"));
        }

        public void DoubleClickOnCard()
        {
            actions.DoubleClick(card).Perform();
            driver.WaitFor(1);
        }

        public void Toggle()
		{
            toggle.Click();
            driver.WaitFor(1);
		}

        public bool IsCollapsed()
		{
            return !toggle.HasClass("is-opened");
		}
    }
}