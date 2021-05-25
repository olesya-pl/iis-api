using Iis.DataModel;
using Iis.Domain.Users;
using Xunit;

namespace Iis.UnitTests.TestHelpers
{
    public static class UserTestHelper
    {
        public static void AssertUserEntityMappedToUserCorrectly(UserEntity source, User destination)
        {
            Assert.Equal(source.Id, destination.Id);
            Assert.Equal(source.LastName, destination.LastName);
            Assert.Equal(source.FirstName, destination.FirstName);
            Assert.Equal(source.Patronymic, destination.Patronymic);
            Assert.Equal(source.Username, destination.UserName);
            Assert.Equal(source.PasswordHash, destination.PasswordHash);
            Assert.Equal(source.IsBlocked, destination.IsBlocked);
        }
    }
}
