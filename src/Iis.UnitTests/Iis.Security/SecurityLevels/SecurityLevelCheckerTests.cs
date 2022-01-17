using System;
using System.Collections.Generic;
using System.Text;
using Iis.Security.SecurityLevels;
using Xunit;

namespace Iis.UnitTests.Iis.Security.SecurityLevels
{
    public class SecurityLevelCheckerTests
    {
        [Fact]
        public void GetStringCodeTest_OneParam()
        {
            var checker = GetChecker();
            Assert.Equal("2:21;", checker.GetStringCode(false, 21));
            Assert.Equal("1:11;", checker.GetStringCode(false, 11));
            Assert.Equal("11:111;", checker.GetStringCode(false, 111));
            Assert.Equal("0:1;", checker.GetStringCode(false, 1));
        }
        [Fact]
        public void GetStringCodeTest_ManyParams()
        {
            var checker = GetChecker();
            Assert.Equal("2:21;11:111;", checker.GetStringCode(false, 21, 111));
            Assert.Equal("1:11;11:111;", checker.GetStringCode(false, 11, 111));
            Assert.Equal("0:1;2:22;", checker.GetStringCode(false, 1, 22));
        }
        [Fact]
        public void GetStringCodeTest_IncludeAll_OneParam()
        {
            var checker = GetChecker();
            Assert.Equal("0:2;2:21;", checker.GetStringCode(true, 21));
            Assert.Equal("0:1;1:11;11:111,112;", checker.GetStringCode(true, 11));
            Assert.Equal("0:1;1:11;11:111;", checker.GetStringCode(true, 111));
            Assert.Equal("0:1;1:11,12;11:111,112;", checker.GetStringCode(true, 1));
        }
        [Fact]
        public void GetStringCodeTest_IncludeAll_ManyParams()
        {
            var checker = GetChecker();
            Assert.Equal("0:1,2;1:11;2:21;11:111;", checker.GetStringCode(true, 21, 111));
            Assert.Equal("0:1;1:11;11:111,112;", checker.GetStringCode(true, 11, 111));
            Assert.Equal("0:1,2;1:11,12;2:22;11:111,112;", checker.GetStringCode(true, 1, 22));
        }

        private SecurityLevelChecker GetChecker() =>
            new SecurityLevelChecker(SecurityLevelModels.GetTestModel1());
    }
}
