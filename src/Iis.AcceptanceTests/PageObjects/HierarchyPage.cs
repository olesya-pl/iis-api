using Iis.AcceptanceTests.Helpers;
using Iis.AcceptanceTests.PageObjects.Controls;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;

namespace Iis.AcceptanceTests.PageObjects
{
    public class HierarchyCard
    {
        private readonly IWebDriver driver;

        private readonly IWebElement card;

        private readonly IWebElement expand;

        private readonly Actions actions;

        public bool Displayed => card.Displayed;

        public HierarchyCard(IWebDriver driver, string title)
        {
            this.driver = driver;
            actions = new Actions(driver);
            card = driver.FindElement(By.XPath($@"//div[contains(text(),""{title}"")]"));
            expand = driver.FindElement(By.XPath($@"//div[contains(text(),""{title}"")]/following::*"));
        }

        public void DoubleClickOnCard()
        {
            actions.DoubleClick(card).Perform();
            driver.WaitFor(1);
        }

        public void Expand()
        {
            actions.DoubleClick(expand).Perform();
            driver.WaitFor(2);
        }
    }
}