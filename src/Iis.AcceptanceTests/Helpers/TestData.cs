using System;
using System.Configuration;
using System.IO;
using System.Reflection;
using DocumentFormat.OpenXml.Office.CoverPageProps;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Xunit;

namespace AcceptanceTests.Helpers
{
    public static class TestData
    {
        public static string BaseAddress;
        public static string BaseApiAddress;
        public static string DefaultUserName;
        public static string DefaultUserPassword;
        public static string RemoteWebDriverUrl;

        static TestData()
        {
            var environmentVariable = Environment.GetEnvironmentVariable("TargetEnvironment");
            TargetEnvironment targetEnvironment = (!string.IsNullOrWhiteSpace(environmentVariable)) ?
                Enum.Parse<TargetEnvironment>(environmentVariable)
                : TargetEnvironment.Dev3;
            if (targetEnvironment != TargetEnvironment.Dev3 && string.IsNullOrWhiteSpace(environmentVariable))
                Console.Out.WriteLine($"YOU ARE TESTING ON {targetEnvironment}!!!");
            ReadTestData(targetEnvironment.ToString());
            if (string.IsNullOrWhiteSpace(environmentVariable))
            {
                RemoteWebDriverUrl = "http://iis-test-selenoid1.contour.net:8081/wd/hub";
            }
        }

        private static void ReadTestData(string targetEnvironment)
        {
            var configurationRoot = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("targetEnvironmentConfig.json")
                .Build();
            var configurationSection = configurationRoot.GetSection(targetEnvironment);

            BaseAddress = configurationSection.GetSection("BaseAddress").Value;
            BaseApiAddress = configurationSection.GetSection("BaseApiAddress").Value;
            DefaultUserName = configurationSection.GetSection("DefaultUserName").Value;
            DefaultUserPassword = configurationSection.GetSection("DefaultUserPassword").Value;
            RemoteWebDriverUrl = configurationSection.GetSection("RemoteWebDriverUrl").Value;
        }
    }

    public enum TargetEnvironment
    {
        Local,
        Dev,
        Dev2,
        Dev3,
        QA,
        ContourStage,
        PogliyaStage,
        Demo
    }
}
