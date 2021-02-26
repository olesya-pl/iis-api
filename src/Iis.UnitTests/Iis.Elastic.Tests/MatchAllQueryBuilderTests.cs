using Xunit;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using FluentAssertions;
using FluentAssertions.Json;

using Iis.Elastic.SearchQueryExtensions;

namespace Iis.UnitTests.Iis.Elastic.Tests
{
    public class MatchAllQueryBuilderTests
    {
        [Fact]
        public void MatchAllQuery_Success()
        {
            var expected = JObject.Parse("{\"_source\":[\"*\"], \"from\":10,\"size\":10,\"query\": {\"match_all\": {}}}");

            var actual = new MatchAllQueryBuilder()
                        .WithPagination(10, 10)
                        .BuildSearchQuery();

            actual.Should().BeEquivalentTo(expected);
        }
    }
}