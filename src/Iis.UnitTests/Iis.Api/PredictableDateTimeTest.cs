using System;
using System.Collections.Generic;
using System.Text;
using Iis.Api.GraphQL.Entities.ObjectTypes;
using Xunit;

namespace Iis.UnitTests.Iis.Api
{
    public class PredictableDateTimeTest
    {
        [Fact]
        public void Serialize_ReturnsExpectedFormat()
        {
            var sut = new PredictableDateType();
            var res = sut.Serialize(new DateTime(2021, 04, 27, 15, 01, 38, 673));
            Assert.Equal("2021-04-27T15:01:38.673", res.ToString());
        }
            
    }
}
