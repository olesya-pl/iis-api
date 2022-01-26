using System;
using System.Collections.Generic;
using System.Text;
using FluentAssertions;
using Iis.Elastic.SearchQueryExtensions;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Iis.UnitTests.Iis.Elastic.Tests
{
    public class DisjunctionQueryBuilderTests
    {
        [Fact]
        public void DisjunctionQueryBuilder_MutipleFields()
        {
            var expected = JObject.Parse("{\"_source\": [\"*\"],\"size\": 3,\"query\": {\"dis_max\": {\"queries\": [{\"match_phrase_prefix\": {\"__title\": {\"query\": \"3 омсб\"}}},{\"match_phrase_prefix\": { \"title\": { \"query\": \"3 омсб\" }}}, { \"match_phrase_prefix\": { \"commonInfo.RealNameShort\": { \"query\": \"3 омсб\"}}}] }},\"from\": 0}");

            var actual = new DisjunctionQueryBuilder("3 омсб", new string[]
            {
                "__title",
                "title",
                "commonInfo.RealNameShort"
            })
                        .WithPagination(0, 3)
                        .BuildSearchQuery();

            actual.Should().BeEquivalentTo(expected);
        }
    }
}
