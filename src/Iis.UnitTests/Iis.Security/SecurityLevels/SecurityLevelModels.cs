using Iis.Security.SecurityLevels;
using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.UnitTests.Iis.Security.SecurityLevels
{
    internal static class SecurityLevelModels
    {
        internal static SecurityLevel GetTestModel1() =>
            new SecurityLevel(
                "top",
                0,
                new List<SecurityLevel>
                {
                    new SecurityLevel(
                        "item1",
                        1,
                        new List<SecurityLevel>
                        {
                            new SecurityLevel(
                                "item11",
                                11,
                                new List<SecurityLevel>
                                {
                                    new SecurityLevel("item111", 111),
                                    new SecurityLevel("item112", 112)
                                }),
                            new SecurityLevel("item12", 12)
                        }),
                    new SecurityLevel(
                        "item2",
                        2,
                        new List<SecurityLevel>
                        {
                            new SecurityLevel("item21", 21),
                            new SecurityLevel("item22", 22)
                        }),
                });
    }
}
