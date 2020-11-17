using System;
using Xunit;
using Newtonsoft.Json.Linq;
using FluentAssertions;
using FluentAssertions.Json;

using Iis.Elastic.SearchQueryExtensions;

namespace Iis.UnitTests.Iis.Elastic.Tests
{
    public class BoolQueryBuilderTests
    {
        [Fact]
        public void BoolQuery_NoConditions()
        {
            var expected = JObject.Parse("{\"_source\":[\"*\"], \"from\":10,\"size\":10,\"query\": {\"bool\": {\"should\": []}}}");

            var actual = new BoolQueryBuilder()
                        .WithPagination(10, 10)
                        .Build();

            actual.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void BoolQuery_NoConditions_WithShould()
        {
            var expected = JObject.Parse("{\"_source\":[\"*\"], \"from\":10,\"size\":10,\"query\": {\"bool\": {\"should\": []}}}");

            var actual = new BoolQueryBuilder()
                        .WithPagination(10, 10)
                        .WithShould()
                        .Build();

            actual.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void BoolQuery_NoConditions_WithMust()
        {
            var expected = JObject.Parse("{\"_source\":[\"*\"], \"from\":10,\"size\":10,\"query\": {\"bool\": {\"must\": []}}}");

            var actual = new BoolQueryBuilder()
                        .WithPagination(10, 10)
                        .WithMust()
                        .Build();

            actual.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void BoolQuery_DocumentListOneDocumentId()
        {
            var expected = JObject.Parse("{\"_source\":[\"*\"], \"from\":10,\"size\":10,\"query\": {\"bool\": {\"should\": [{\"ids\":{\"values\":[\"6f5f83e4093f407fa843f235f62a93c8\"]}}]}}}");

            var actual = new BoolQueryBuilder()
                        .WithPagination(10, 10)
                        .WithDocumentList(new Guid[]{Guid.Parse("6f5f83e4093f407fa843f235f62a93c8")})
                        .Build();

            actual.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void BoolQuery_DocumentListTwoDocumentId()
        {
            var expected = JObject.Parse("{\"_source\":[\"*\"], \"from\":10,\"size\":10,\"query\": {\"bool\": {\"should\": [{\"ids\":{\"values\":[\"6f5f83e4093f407fa843f235f62a93c8\", \"0208a7dc4e46477997ed9b50ec24ad88\"]}}]}}}");

            var actual = new BoolQueryBuilder()
                        .WithPagination(10, 10)
                        .WithDocumentList(new Guid[]{Guid.Parse("6f5f83e4093f407fa843f235f62a93c8"), Guid.Parse("0208a7dc4e46477997ed9b50ec24ad88")})
                        .Build();

            actual.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void BoolQuery_ExactQuery()
        {
            var expected = JObject.Parse("{\"_source\":[\"*\"], \"from\":10,\"size\":10,\"query\": {\"bool\": {\"should\": [{\"query_string\":{\"query\": \"Source:iis.api\", \"lenient\": true}}]}}}");

            var actual = new BoolQueryBuilder()
                        .WithPagination(10, 10)
                        .WithExactQuery("Source:iis.api")
                        .Build();

            actual.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void BoolQuery_ExactQueryWithDocumentListOneDocumentId()
        {
            var expected = JObject.Parse("{\"_source\":[\"*\"], \"from\":10,\"size\":10,\"query\": {\"bool\": {\"should\": [{\"ids\":{\"values\":[\"6f5f83e4093f407fa843f235f62a93c8\"]}}, {\"query_string\":{\"query\": \"Source:iis.api\", \"lenient\": true}}]}}}");

            var actual = new BoolQueryBuilder()
                        .WithPagination(10, 10)
                        .WithDocumentList(new Guid[]{Guid.Parse("6f5f83e4093f407fa843f235f62a93c8")})
                        .WithExactQuery("Source:iis.api")
                        .Build();

            actual.Should().BeEquivalentTo(expected);
        }
    }
}