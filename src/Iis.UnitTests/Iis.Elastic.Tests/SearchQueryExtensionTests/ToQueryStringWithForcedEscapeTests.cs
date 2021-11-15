using AutoFixture.Xunit2;
using FluentAssertions;
using Iis.Elastic.SearchQueryExtensions;
using Iis.Interfaces.Elastic;
using Xunit;

namespace Iis.UnitTests.Iis.Elastic.Tests.SearchQueryExtensionTests
{
    public class ToQueryStringWithForcedEscapeTests
    {
        [Theory]
        [MemberData(nameof(ElasticFilterData.GetElasticFilterData), MemberType = typeof(ElasticFilterData))]
        public void ToQueryStringWithForcedEscape_ShouldReturnValidQuery(ElasticFilter filter, string expected)
        {
            var actual = filter.ToQueryString();

            actual.Should().Be(expected);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(SearchQueryExtension.Wildcard)]
        public void ToQueryStringWithForcedEscape_ShouldReturnWildcard(string suggestion)
        {
            var filter = new ElasticFilter
            {
                Limit = 10,
                Offset = 1,
                Suggestion = suggestion
            };

            var result = filter.ToQueryStringWithForcedEscape(false);

            result.Should().Be(SearchQueryExtension.Wildcard);
        }

        [Theory]
        [AutoData]
        public void ToQueryStringWithForcedEscape_ShouldReturnSuggestion(string suggestion)
        {
            var filter = new ElasticFilter
            {
                Limit = 10,
                Offset = 1,
                Suggestion = suggestion
            };

            var result = filter.ToQueryStringWithForcedEscape(false);

            result.Should().Be(suggestion);
        }

        [Theory]
        [AutoData]
        public void ToQueryStringWithForcedEscape_ShouldReturnFuzzinessSuggestion(string suggestion)
        {
            var filter = new ElasticFilter
            {
                Limit = 10,
                Offset = 1,
                Suggestion = suggestion
            };
            var expected = $"\"{suggestion}\" OR {suggestion}~";

            var result = filter.ToQueryStringWithForcedEscape(true);

            result.Should().Be(expected);
        }

        [Theory]
        [MemberData(nameof(ElasticFilterData.GetElasticFilterDataWithEscape), MemberType = typeof(ElasticFilterData))]
        public void ToQueryStringWithForcedEscape_ShouldEscape(ElasticFilter filter, bool applyFuzziness, string expected)
        {
            var actual = filter.ToQueryStringWithForcedEscape(applyFuzziness);

            actual.Should().Be(expected);
        }
    }
}