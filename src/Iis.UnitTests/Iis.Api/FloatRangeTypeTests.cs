using IIS.Core.GraphQL.Entities.ObjectTypes;
using Xunit;

namespace Iis.UnitTests.Iis.Api
{
    public class FloatRangeTypeTests
    {
        [Fact]
        public void FloatRangeType_CanBeDeserialized()
        {
            var sut = new FloatRangeType();
            sut.TryDeserialize("1-112", out var res);
            Assert.Equal("1-112", res);
        }

        [Fact]
        public void IntRangeType_CanBeDeserialized()
        {
            var sut = new IntegerRangeType();
            sut.TryDeserialize("1-112", out var res);
            Assert.Equal("1-112", res);
        }
    }
}
