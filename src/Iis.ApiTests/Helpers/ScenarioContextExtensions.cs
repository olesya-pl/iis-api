using OpenQA.Selenium;
using TechTalk.SpecFlow;

namespace AcceptanceTests.Helpers
{
    public static class ScenarioContextExtensions
    {
        public static string GetAuthToken(this ScenarioContext scenarioContext)
        {
            return scenarioContext.Get<string>("authToken");
        }

        public static void SetAuthToken(this ScenarioContext scenarioContext, string token)
        {
            scenarioContext.Add("authToken", token);
        }

        public static T GetResponse<T>(this ScenarioContext scenarioContext, string key)
        {
            return scenarioContext.Get<T>(key);
        }

        public static void SetResponse<T>(this ScenarioContext scenarioContext, string key, T value)
        {
            scenarioContext.Add(key, value);
        }

        public static void SetDriver(this ScenarioContext scenarioContext, IWebDriver driver)
        {
            scenarioContext.Add("webDriver", driver);
        }
        public static IWebDriver GetDriver(this ScenarioContext scenarioContext)
        {
            return scenarioContext.Get<IWebDriver>("webDriver");
        }
    }
}