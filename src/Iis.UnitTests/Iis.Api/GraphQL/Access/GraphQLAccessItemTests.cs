using Iis.Api.GraphQL.Access;
using Iis.Interfaces.Roles;
using Xunit;

namespace Iis.UnitTests.Iis.Api.GraphQL.Access
{
    public class GraphQLAccessItemTests
    {
        [Fact]
        public void IsMatch_ItemsEqual_ReturnsTrue()
        {
            var sut = new GraphQLAccessItem(AccessKind.FreeForAll, AccessOperation.None, @"getEntityTypes", @"getEntityTypeIcons");
            Assert.True(sut.IsMatch("getEntityTypes"));
        }

        [Fact]
        public void IsMatch_NoMatchingItems_ReturnsFalse()
        {
            var sut = new GraphQLAccessItem(AccessKind.FreeForAll, AccessOperation.None, @"getEntityTypes", @"getEntityTypeIcons");
            Assert.False(sut.IsMatch("getEntityIcons"));
        }

        [Fact]
        public void IsMatch_RegexIsRespected()
        {
            var sut = new GraphQLAccessItem(AccessKind.Entity, AccessOperation.Create, @"createEntity(?!Event)");
            Assert.False(sut.IsMatch("createEntityEvent"));
            Assert.True(sut.IsMatch("createEntityMilitaryOrganization"));
        }
    }
}
