using System;
using System.Reflection;
using AcceptanceTests.Helpers;
using BoDi;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Remote;
using TechTalk.SpecFlow;

namespace AcceptanceTests
{
    [Binding]
    public class InitiateWebDriver
    {
        private readonly IObjectContainer container;

        public InitiateWebDriver(IObjectContainer container)
        {
            this.container = container;
        }

        [BeforeScenario("UI")]
        public void CreateWebDriver()
        {
            var driverOptions = new ChromeOptions();

            var runName = Assembly.GetExecutingAssembly().GetName().Name;

            driverOptions.AddAdditionalCapability("name", runName, true);
            driverOptions.AddAdditionalCapability("enableVNC", true, true);
            driverOptions.AddAdditionalCapability("screenResolution", "1920x1080x24", true);

            // var driver = new ChromeDriver();
            // var driver = new RemoteWebDriver(new Uri("http://192.168.88.114:4444/wd/hub"), driverOptions);
            var driver = new RemoteWebDriver(new Uri(TestData.RemoteWebDriverUrl), driverOptions);
            //homeUrl = "http://qa.contour.net/";
            driver.Manage().Window.Maximize();
            container.RegisterInstanceAs<IWebDriver>(driver);
        }

        [AfterScenario("UI")]
        public void DestroyWebDriver()
        {
            var driver = container.Resolve<IWebDriver>();

            if (driver != null)
            {
                driver.Quit();
                driver.Dispose();
            }
        }
    }
}
