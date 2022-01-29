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

        [Fact]
        public void AccessGranted_Equality()
        {
            var checker = GetChecker();
            Assert.True(checker.AccessGranted(new[] { 11 }, new[] { 11 }));
            Assert.True(checker.AccessGranted(new[] { 111 }, new[] { 111 }));
            Assert.True(checker.AccessGranted(new[] { 0 }, new[] { 0 }));
        }
        [Fact]
        public void AccessGranted_Parent()
        {
            var checker = GetChecker();
            Assert.True(checker.AccessGranted(new[] { 0 }, new[] { 11 }));
            Assert.True(checker.AccessGranted(new[] { 1 }, new[] { 11 }));
            Assert.True(checker.AccessGranted(new[] { 1 }, new[] { 111 }));
            Assert.True(checker.AccessGranted(new[] { 11 }, new[] { 111 }));
        }
        [Fact]
        public void AccessGranted_Child()
        {
            var checker = GetChecker();
            Assert.True(checker.AccessGranted(new[] { 11 }, new[] { 1 }));
            Assert.True(checker.AccessGranted(new[] { 111 }, new[] { 1 }));
            Assert.True(checker.AccessGranted(new[] { 111 }, new[] { 11 }));
        }
        [Fact]
        public void AccessGranted_Brothers()
        {
            var checker = GetChecker();
            Assert.True(checker.AccessGranted(new[] { 11 }, new[] { 11, 12 }));
            Assert.True(checker.AccessGranted(new[] { 12 }, new[] { 11, 12 }));
            Assert.True(checker.AccessGranted(new[] { 111 }, new[] { 111, 112 }));
            Assert.True(checker.AccessGranted(new[] { 112 }, new[] { 111, 112 }));
        }
        [Fact]
        public void AccessGranted_GroupLevels()
        {
            var checker = GetChecker();
            Assert.False(checker.AccessGranted(new[] { 1 }, new[] { 1, 2 }));
            Assert.False(checker.AccessGranted(new[] { 2 }, new[] { 1, 2 }));
            Assert.True(checker.AccessGranted(new[] { 1, 2 }, new[] { 1, 2 }));
            Assert.True(checker.AccessGranted(new[] { 1, 22 }, new[] { 1, 2 }));
            Assert.True(checker.AccessGranted(new[] { 111, 21 }, new[] { 1, 2 }));
        }
        [Fact]
        public void AccessGranted_Combined()
        {
            var checker = GetChecker();
            Assert.True(checker.AccessGranted(new[] { 1, 21 }, new[] { 1, 21, 22 }));
            Assert.True(checker.AccessGranted(new[] { 11, 2 }, new[] { 1, 21, 22 }));
            Assert.True(checker.AccessGranted(new[] { 111, 22 }, new[] { 1, 21, 22 }));
        }
        [Fact]
        public void AccessGranted_False_Single()
        {
            var checker = GetChecker();
            Assert.False(checker.AccessGranted(new[] { 2 }, new[] { 1 }));
            Assert.False(checker.AccessGranted(new[] { 21 }, new[] { 1 }));
            Assert.False(checker.AccessGranted(new[] { 22 }, new[] { 11 }));
        }
        [Fact]
        public void AccessGranted_False_Combined()
        {
            var checker = GetChecker();
            Assert.False(checker.AccessGranted(new[] { 2 }, new[] { 1, 21, 22 }));
            Assert.False(checker.AccessGranted(new[] { 1 }, new[] { 1, 21, 22 }));
            Assert.False(checker.AccessGranted(new[] { 11, 112 }, new[] { 2 }));
        }

        private SecurityLevelChecker GetChecker() =>
            new SecurityLevelChecker(SecurityLevelModels.GetTestModel1());
    }
}
