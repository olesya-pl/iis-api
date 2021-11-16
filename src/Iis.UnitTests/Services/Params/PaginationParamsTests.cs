using System;
using Xunit;
using FluentAssertions;
using Iis.Interfaces.Elastic;

namespace Iis.UnitTests.Services.Params
{
    public class PaginationParamsTests
    {
        [Fact]
        public void PageAndSizeSuccess()
        {
            var actual = new PaginationParams(1, 50);

            actual.Page.Should().Be(1);
            actual.Size.Should().Be(50);
        }

        [Fact]
        public void PageShouldNotBeZero()
        {
           Action act = () => new PaginationParams(0, 50);

            act.Should()
                .Throw<ArgumentOutOfRangeException>()
                .WithMessage("Parameter should be above zero. (Parameter 'page')\r\nActual value was 0.");
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(-5)]
        [InlineData(-10)]
        [InlineData(-15)]
        [InlineData(-20)]
        public void PageShouldNotBeLessZero(int page)
        {
            Action act = () => new PaginationParams(page, 50);

            act.Should()
                .Throw<ArgumentOutOfRangeException>()
                .WithMessage($"Parameter should be above zero. (Parameter 'page')\r\nActual value was {page}.");
        }

        [Fact]
        public void SizeShouldNotBeZero()
        {
            Action act = () => new PaginationParams(1, 0);

            act.Should()
                .Throw<ArgumentOutOfRangeException>()
                .WithMessage("Parameter should be above zero. (Parameter 'size')\r\nActual value was 0.");

        }

        [Theory]
        [InlineData(-1)]
        [InlineData(-5)]
        [InlineData(-10)]
        [InlineData(-15)]
        [InlineData(-20)]
        public void SizeShouldNotBeLessZero(int size)
        {
            Action act = () => new PaginationParams(1, size);

            act.Should()
                .Throw<ArgumentOutOfRangeException>()
                .WithMessage($"Parameter should be above zero. (Parameter 'size')\r\nActual value was {size}.");
        }

        [Theory]
        [InlineData(1, 50, 0)]
        [InlineData(2, 50, 50)]
        public void ElasticPage(int page, int size, int expectedFrom)
        {
            var actual = new PaginationParams(page, size);

            var (elasticFrom, elasticSize) = actual.ToElasticPage();

            elasticFrom.Should().Be(expectedFrom);
            elasticSize.Should().Be(size);
        }
    }
}