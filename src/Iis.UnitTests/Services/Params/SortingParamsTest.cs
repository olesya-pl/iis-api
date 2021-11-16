using System;
using Xunit;
using FluentAssertions;
using Iis.Interfaces.Materials;

namespace Iis.UnitTests.Services.Params
{
    public class SortingParamsTests
    {
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void Success_EmptyOrderToASCAsDefault(string sortOrder)
        {
            var sorting = new SortingParams(null, sortOrder);

            sorting.ColumnName.Should().BeNull();
            sorting.Order.Should().Be("asc");
        }
        [Theory]
        [InlineData("type", "asc")]
        [InlineData("type", "desc")]
        public void Success_AllProperiesHasValues(string columnName, string sortOrder)
        {
            var sorting = new SortingParams(columnName, sortOrder);

            sorting.ColumnName.Should().Be(columnName);
            sorting.Order.Should().Be(sortOrder);
        }
    }
}