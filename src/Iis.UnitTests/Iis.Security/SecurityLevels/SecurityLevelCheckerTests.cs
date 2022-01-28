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
        public void GetObjectElasticCodeTest_OneParam()
        {
            var checker = GetChecker();
            Assert.Equal("2:21;", checker.GetObjectElasticCode(21));
            Assert.Equal("1:11;", checker.GetObjectElasticCode(11));
            Assert.Equal("11:111;", checker.GetObjectElasticCode(111));
            Assert.Equal("0:1;", checker.GetObjectElasticCode(1));
        }
        [Fact]
        public void GetObjectElasticCodeTest_ManyParams()
        {
            var checker = GetChecker();
            Assert.Equal("2:21;11:111;", checker.GetObjectElasticCode(21, 111));
            Assert.Equal("1:11;11:111;", checker.GetObjectElasticCode(11, 111));
            Assert.Equal("0:1;2:22;", checker.GetObjectElasticCode(1, 22));
        }
        [Fact]
        public void GetObjectElasticCodeTest_GroupLevels()
        {
            var checker = GetChecker();
            Assert.Equal("0:1;0:2;", checker.GetObjectElasticCode(1, 2));
            Assert.Equal("0:1;0:2;2:21;", checker.GetObjectElasticCode(1, 2, 21));
        }
        [Fact]
        public void GetUserElasticCodeTest_OneParam()
        {
            var checker = GetChecker();
            Assert.Equal("0:2;2:21;", checker.GetUserElasticCode(21));
            Assert.Equal("0:1;1:11;11:111,112;", checker.GetUserElasticCode(11));
            Assert.Equal("0:1;1:11;11:111;", checker.GetUserElasticCode(111));
            Assert.Equal("0:1;1:11,12;11:111,112;", checker.GetUserElasticCode(1));
        }
        [Fact]
        public void GetUserElasticCodeTest_ManyParams()
        {
            var checker = GetChecker();
            Assert.Equal("0:1,2;1:11;2:21;11:111;", checker.GetUserElasticCode(21, 111));
            Assert.Equal("0:1;1:11;11:111,112;", checker.GetUserElasticCode(11, 111));
            Assert.Equal("0:1,2;1:11,12;2:22;11:111,112;", checker.GetUserElasticCode(1, 22));
        }

        private SecurityLevelChecker GetChecker() =>
            new SecurityLevelChecker(SecurityLevelModels.GetTestModel1());
    }
}
