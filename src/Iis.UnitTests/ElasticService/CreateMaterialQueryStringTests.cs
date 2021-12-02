using System;
using Iis.Interfaces.Elastic;
using Iis.Interfaces.Materials;
using Iis.Interfaces.Common;
using Xunit;
using Iis.Elastic.SearchQueryExtensions;
using FluentAssertions;

namespace Iis.UnitTests.CreateMaterialQueryStringTests
{
    public class CreateMaterialQueryStringTests
    {
        [Fact]
        public void DateRangeIsProvided_HoursAndMinutesRespected()
        {
            //arrange
            var searchParams = new SearchParams
            {
                Page = new PaginationParams(1, 50),
                CreatedDateRange = new DateRange(new DateTime(2021, 11, 18, 10, 00, 00), new DateTime(2021, 11, 18, 12, 00, 00))
            };

            //act
            var queryString = SearchQueryExtension.CreateMaterialsQueryString(searchParams);

            //assert
            queryString.Should().Be("((ParentId:NULL)) AND CreatedDate:[2021-11-18T10:00:00Z TO 2021-11-18T12:00:00Z]");
        }
    }
}
