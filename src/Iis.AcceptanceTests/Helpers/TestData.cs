using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.AcceptanceTests.Helpers
{
    public static class TestData
    {
        public static string BaseAddress;
        public static string BaseApiAddress;

        static TestData()
        {
            BaseAddress = "http://qa.contour.net";
            BaseApiAddress = "http://qa.contour.net:5000";
        }
    }

    public enum TargetEnvironment
    {
        Local,
        Dev,
        QA,
        Stage,
        Dev3
    }
}
