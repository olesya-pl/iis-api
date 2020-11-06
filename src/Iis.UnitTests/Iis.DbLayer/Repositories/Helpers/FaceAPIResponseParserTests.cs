using System.Linq;
using Xunit;
using FluentAssertions;
using FluentAssertions.Json;
using Iis.DbLayer.Repositories.Helpers;

namespace Iis.UnitTests.Iis.DbLayer.Repositories.Helpers
{
    public class FaceAPIResponseParserTests
    {
        [Fact]
        public void GetEncoding_WithErrorContentReturnsNull()
        {
            var content = "{\"помилка\": \"Response status code does not indicate success: 500 (Internal Server Error).\"}";

            var imageVector = FaceAPIResponseParser.GetEncoding(content);

            imageVector.Should().BeNull();
        }
        [Fact]
        public void GetEncoding_WithArrayAsContentReturnsArray()
        {
            var content = "[  -0.36,  0.16,  0.16,  0.12]";

            var expected = new decimal[]{-0.36m, 0.16m, 0.16m, 0.12m};

            var imageVector = FaceAPIResponseParser.GetEncoding(content);

            imageVector.Should().Equal(expected);
        }
        [Fact]
        public void GetEncoding_WithObjectAsContentReturnsArray()
        {
            var content = "{ \"time\": 179, \"result\": [ { \"location\": [ [ 187, 96 ], [ 262, 171 ] ], \"encoding\": [ -0.086, 0.069, 0.063, -0.024 ] } ] }";

            var expected = new decimal[]{-0.086m, 0.069m, 0.063m, -0.024m};

            var imageVector = FaceAPIResponseParser.GetEncoding(content);

            imageVector.Should().Equal(expected);
        }
        [Fact]
        public void GetEncoding_NoResultPropertyInContentReturnsNull()
        {
            var content = "{ \"time\": 179 }";

            var imageVector = FaceAPIResponseParser.GetEncoding(content);

            imageVector.Should().BeNull();
        }
        [Fact]
        public void GetEncoding_EmptyResultPropertyInContentReturnsNull()
        {
            var content = "{ \"time\": 179, \"result\": [] }";

            var imageVector = FaceAPIResponseParser.GetEncoding(content);

            imageVector.Should().BeNull();
        }
        [Fact]
        public void GetEncoding_EmptyEncodingArrayInContentReturnsNull()
        {
            var content = "{ \"time\": 179, \"result\": [ { \"location\": [ [ 187, 96 ], [ 262, 171 ] ], \"encoding\": [] } ] }";

            var imageVector = FaceAPIResponseParser.GetEncoding(content);

            imageVector.Should().BeNull();
        }
        [Fact]
        public void GetEncoding_ResultWithTwoItemsInContentReturnsFirst()
        {
            var content = "{ \"time\": 179, \"result\": [ { \"location\": [ [ 187, 96 ], [ 262, 171 ] ], \"encoding\": [] } ] }";

            var imageVector = FaceAPIResponseParser.GetEncoding(content);

            imageVector.Should().BeNull();
        }
    }
}