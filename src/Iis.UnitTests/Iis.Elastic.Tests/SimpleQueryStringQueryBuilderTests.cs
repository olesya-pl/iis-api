using System;
using Xunit;
using Newtonsoft.Json.Linq;
using FluentAssertions;
using FluentAssertions.Json;
using Iis.Elastic.SearchQueryExtensions;

namespace Iis.UnitTests.Iis.Elastic.Tests
{
    public class SimpleQueryStringQueryBuilderTests
    {
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void SimpleQueryStringQuery_NoQueryString(string queryString)
        {
            Action act = () => new SimpleQueryStringQueryBuilder(queryString);

            act.Should()
                .Throw<ArgumentException>()
                .WithMessage("Value cannot be null. (Parameter 'queryString')");
        }

        [Fact]
        public void SimpleQueryStringQuery_Success()
        {
            var expected = JObject.Parse("{\"_source\": [\"*\"], \"query\":{\"query_string\":{\"query\":\"fieldName:fieldValue\"}}}");

            var actual = new SimpleQueryStringQueryBuilder("fieldName:fieldValue")
                        .BuildSearchQuery();

            actual.Should().BeEquivalentTo(expected);
        }
    }
}