﻿using TechTalk.SpecFlow;

namespace AcceptanceTests.Steps
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
    }
}