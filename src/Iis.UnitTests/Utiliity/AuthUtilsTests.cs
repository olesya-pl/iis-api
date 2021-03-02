using Iis.Utility;
using Xunit;

namespace Iis.UnitTests.Utiliity
{
    public class AuthUtilsTests
    {
        [Theory]
        [InlineData("elastic", "112233", "Basic ZWxhc3RpYzoxMTIyMzM=")]
        public void GenerateBasicAuthHeaderValue(string login, string password, string expected)
        {
            Assert.Equal(expected, AuthUtils.GenerateBasicAuthHeaderValue(login, password));
        }
    }
}
