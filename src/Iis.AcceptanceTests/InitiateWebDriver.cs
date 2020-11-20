using System;
using BoDi;
using Iis.AcceptanceTests.Helpers;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Remote;
using TechTalk.SpecFlow;

namespace Iis.AcceptanceTests.UISteps
{
    [Binding]
    public class InitiateWebDriver
    {
        private readonly IObjectContainer container;

        public InitiateWebDriver(IObjectContainer container)
        {
            this.container = container;
        }
        [BeforeScenario]
        public void CreateWebDriver()
        {
            var driverOptions = new ChromeOptions();

            var runName = GetType().Assembly.GetName().Name;
            var timestamp = $"{DateTime.Now:yyyyMMdd.HHmm}";

            driverOptions.AddAdditionalCapability("name", runName, true);
            driverOptions.AddAdditionalCapability("enableVNC", true, true);
            driverOptions.AddAdditionalCapability("screenResolution", "1920x1080x24", true);

            //var driver = new ChromeDriver();
            var driver = new RemoteWebDriver(new Uri("http://192.168.88.114:4444/wd/hub"), driverOptions);
            //var driver = new RemoteWebDriver(new Uri("http://localhost:4444/wd/hub"), driverOptions);
            //homeUrl = "http://qa.contour.net/";
            driver.Manage().Window.Maximize();
            container.RegisterInstanceAs<IWebDriver>(driver);
        }

        [AfterScenario]
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
