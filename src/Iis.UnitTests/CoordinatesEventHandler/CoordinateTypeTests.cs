using Xunit;
using FluentAssertions;
using Iis.CoordinatesEventHandler.Types;

namespace Iis.UnitTests.CoordinatesEventHandler
{
    public class CoordinateTypeTests
    {
        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public void ConstructWithNullParameter_Invalid(string value)
        {
            var actual = new Coordinate(value);

            actual
                .Should()
                .NotBeNull()
                .And
                .Match<Coordinate>(e => !e.IsValid && e.Latitude == 0 && e.Longitude == 0);
        }

        [Fact]
        public void ConstructWithOneWord_Invalid()
        {
            var actual = new Coordinate("one_word");

            actual
                .Should()
                .NotBeNull()
                .And
                .Match<Coordinate>(e => !e.IsValid && e.Latitude == 0 && e.Longitude == 0);
        }

        [Fact]
        public void ConstructWithCommaSeparatedTwoWord_Invalid()
        {
            var actual = new Coordinate("one_word, sec_word");

            actual
                .Should()
                .NotBeNull()
                .And
                .Match<Coordinate>(e => !e.IsValid && e.Latitude == 0 && e.Longitude == 0);
        }
        [Fact]
        public void ConstructWithCommaSeparatedDecimals_Valid()
        {
            var lat = 31.31m;
            var lon = 55.55m;
            var actual = new Coordinate($"{lat}, {lon}");

            actual
                .Should()
                .NotBeNull()
                .And
                .Match<Coordinate>(e => e.IsValid && e.Latitude == lat && e.Longitude == lon);
        }
    }
}