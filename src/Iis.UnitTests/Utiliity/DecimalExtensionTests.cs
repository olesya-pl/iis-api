using Xunit;
using Iis.Utility;
using FluentAssertions;
namespace Iis.UnitTests.Utility
{
    public class DecimalExtensionsTests
    {
        [Fact]
        public void PassZero()
        {

            (0.00m).Truncate(2).Should().Equals(0.0m);
        }

        [Fact]
        public void PassZeroWithLowerPrecision()
        {
            (0.0m).Truncate(2).Should().Equals(0.0m);
        }

        [Fact]
        public void PassNumberLowerPrecision()
        {
            (1.2m).Truncate(2).Should().Equals(1.2m);
        }

        [Fact]
        public void PassNegativeNumberNoRound()
        {
            (-1.014m).Truncate(2).Should().Equals(-1.01m);
        }

        [Fact]
        public void PassPositiveNumberGreaterNoRound()
        {
            (1.016m).Truncate(2).Should().Equals(1.01m);
        }

        [Fact]
        public void PassPositiveNumberLowerNoRound()
        {
            (1.014m).Truncate(2).Should().Equals(1.01m);
        }
    }
}