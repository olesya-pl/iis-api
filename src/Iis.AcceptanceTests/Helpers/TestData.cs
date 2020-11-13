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
            BaseAddress = "http://dev3.contour.net";
            BaseApiAddress = "http://dev3.contour.net:5000";
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
