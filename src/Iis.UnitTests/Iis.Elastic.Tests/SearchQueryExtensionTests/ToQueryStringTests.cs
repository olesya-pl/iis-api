using AutoFixture.Xunit2;
using FluentAssertions;
using Iis.Elastic.SearchQueryExtensions;
using Iis.Interfaces.Elastic;
using Xunit;

namespace Iis.UnitTests.Iis.Elastic.Tests.SearchQueryExtensionTests
{
    public class ToQueryStringTests
    {
        [Theory]
        [MemberData(nameof(ElasticFilterData.GetElasticFilterData), MemberType = typeof(ElasticFilterData))]
        public void ToQueryString_ShouldReturnValidQuery(ElasticFilter filter, string expected)
        {
            var actual = filter.ToQueryString();

            actual.Should().Be(expected);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(SearchQueryExtension.Wildcard)]
        public void ToQueryString_ShouldReturnWildcard(string suggestion)
        {
            var filter = new ElasticFilter
            {
                Limit = 10,
                Offset = 1,
                Suggestion = suggestion
            };

            var result = filter.ToQueryString();

            result.Should().Be(SearchQueryExtension.Wildcard);
        }

        [Theory]
        [AutoData]
        public void ToQueryString_ShouldReturnSuggestion(string suggestion)
        {
            var filter = new ElasticFilter
            {
                Limit = 10,
                Offset = 1,
                Suggestion = suggestion
            };

            var result = filter.ToQueryString();

            result.Should().Be(suggestion);
        }

        [Theory]
        [AutoData]
        public void ToQueryString_ShouldReturnFuzzinessSuggestion(string suggestion)
        {
            var filter = new ElasticFilter
            {
                Limit = 10,
                Offset = 1,
                Suggestion = suggestion
            };
            var expected = $"\"{suggestion}\" OR {suggestion}~";

            var result = filter.ToQueryString(true);

            result.Should().Be(expected);
        }
    }
}