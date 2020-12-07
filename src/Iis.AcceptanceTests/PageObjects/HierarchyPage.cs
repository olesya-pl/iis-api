using Iis.AcceptanceTests.PageObjects.Controls;
using OpenQA.Selenium;

namespace Iis.AcceptanceTests.PageObjects
{
    public class HierarchyCard
    {
        private readonly IWebDriver driver;

        private readonly IWebElement card;

        private readonly IWebElement expand;


        public HierarchyCard(IWebDriver driver, string title)
        {
            this.driver = driver;
            card = driver.FindElement(By.XPath($@"//div[contains(text(),""{title}"")]"));
            var parent = card.FindElement(By.XPath("./.."));
            expand = parent.FindElement(By.Class("hierarchy-card__expand"));
        }
        //public OOSHierarchy SecurityAgenciesBlock => new OOSHierarchy(driver, By.(""));
    }
}