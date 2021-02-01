using System;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using FluentAssertions;
using FluentAssertions.Json;
using Xunit;

using Iis.Elastic.SearchQueryExtensions;

namespace Iis.UnitTests.Iis.Elastic.Tests
{
    public class SearchQueryExtensionTests
    {
        [Theory]
        [InlineData(new[] { "*" }, 1, 5)]
        [InlineData(new[] { "Id", "Type", "Source" }, 0, 3)]
        public void WithSearchJson_Success(IEnumerable<string> resultFieldList, int from, int size)
        {
            var expected = JObject.Parse("{\"_source\":[" + string.Join(",", resultFieldList.Select(x => $"\"{x}\"")) + "], \"from\":" + from + ",\"size\":" + size + ",\"query\": {}}");

            var actual = SearchQueryExtension.WithSearchJson(resultFieldList, from, size);

            actual.Should().BeEquivalentTo(expected);
        }
        [Theory]
        [InlineData(1, 5)]
        public void WithSearchJson_DefaultResultFieldList(int from, int size)
        {
            var expected = JObject.Parse("{\"_source\":[\"*\"], \"from\":" + from + ",\"size\":" + size + ",\"query\": {}}");

            var actual = SearchQueryExtension.WithSearchJson(null, from, size);

            actual.Should().BeEquivalentTo(expected);
        }
        [Fact]
        public void SetupHighlights_Success()
        {
            var expected = JObject.Parse("{\"highlight\":{\"fields\":{\"*\":{\"type\":\"unified\"}}}}");

            var actual = new JObject().WithHighlights();

            actual.Should().BeEquivalentTo(expected);
        }
        [Fact]
        public void SetupHighlights_Null()
        {
            JObject stubValue = null;

            var actual = stubValue.WithHighlights();

            actual.Should().BeNull();
        }
        [Fact]
        public void SetupHighlights_UpdateExistingProperty()
        {
            var expected = JObject.Parse("{\"highlight\":{\"fields\":{\"*\":{\"type\":\"unified\"}}}}");

            var stubValue = new JObject(
                new JProperty("highlight", "stub_value")
            );

            var actual = stubValue.WithHighlights();

            actual.Should().BeEquivalentTo(expected);
        }
        [Theory]
        [InlineData("fieldName:fieldValue", true)]
        [InlineData("fieldName:fieldValue", false)]
        public void SetupExactQuery_Success(string query, bool lenient)
        {
            var expected = JObject.Parse("{\"_source\": [\"*\"], \"from\": 0, \"size\": 0,\"query\":{\"query_string\":{\"query\":\"" + query + "\", \"lenient\":" + lenient.ToString().ToLower() + "}}}");

            var actual = new ExactQueryBuilder()
                .WithQueryString(query)
                .WithLeniency(lenient)
                .Build();

            actual.Should().BeEquivalentTo(expected);
        }
        [Theory]
        [InlineData("fieldName:fieldValue")]
        public void SetupExactQuery_Success_NoLenient(string query)
        {
            var expected = JObject.Parse("{\"_source\": [\"*\"], \"from\": 0, \"size\": 0,\"query\":{\"query_string\":{\"query\":\"" + query + "\"}}}");

            var actual = new ExactQueryBuilder()
                .WithQueryString(query)
                .Build();

            actual.Should().BeEquivalentTo(expected);
        }
        [Theory]
        [InlineData("fieldName:fieldValue", 14, 88)]
        public void SetupExactQuery_Success_WithPagination(string query, int offset, int size)
        {
            var expected = JObject.Parse("{\"_source\": [\"*\"], \"from\": " + offset + ", \"size\": " + size + ",\"query\":{\"query_string\":{\"query\":\"" + query + "\"}}}");

            var actual = new ExactQueryBuilder()
                .WithQueryString(query)
                .WithPagination(offset, size)
                .Build();

            actual.Should().BeEquivalentTo(expected);
        }
        [Theory]
        [InlineData("fieldName:fieldValue", 21, 69, "Id")]
        public void SetupExactQuery_Success_WithPagination_AndResultFields(string query, int offset, int size, string resultField)
        {
            var expected = JObject.Parse("{\"_source\": [\"" + resultField + "\"], \"from\": " + offset + ", \"size\": " + size + ",\"query\":{\"query_string\":{\"query\":\"" + query + "\"}}}");

            var actual = new ExactQueryBuilder()
                .WithQueryString(query)
                .WithPagination(offset, size)
                .WithResultFields(new[] { resultField })
                .Build();

            actual.Should().BeEquivalentTo(expected);
        }
        [Theory]
        [InlineData("fieldName", "asc")]
        [InlineData("fieldName", "desc")]
        public void SetupSorting_Success(string sortColumn, string sortOrder)
        {
            var expected = JObject.Parse("{\"sort\":[{\"" + sortColumn + "\":{\"order\":\"" + sortOrder + "\"}}]}");

            var actual = new JObject().SetupSorting(sortColumn, sortOrder);

            actual.Should().BeEquivalentTo(expected);
        }
        [Theory]
        [InlineData(null, "asc")]
        [InlineData("fieldName", null)]
        public void SetupSorting_Null(string sortColumn, string sortOrder)
        {
            JObject stubValue = null;

            var actual = stubValue.SetupSorting(sortColumn, sortColumn);

            actual.Should().BeNull();
        }
        [Fact]
        public void SetupSorting_UpdateExistingProperty()
        {
            var expected = JObject.Parse("{\"sort\":[{\"fieldName1\":{\"order\":\"asc\"}}, {\"fieldName2\":{\"order\":\"desc\"}}]}");

            var stubValue = JObject.Parse("{\"sort\":[{\"fieldName1\":{\"order\":\"asc\"}}]}");

            var actual = stubValue.SetupSorting("fieldName2", "desc");

            actual.Should().BeEquivalentTo(expected);
        }
        [Fact]
        public void SetupSorting_SortIsNotArray()
        {
            var expected = JObject.Parse("{\"sort\":{\"fieldName\":\"fieldValue\"}}");

            var stubValue = new JObject(
                new JProperty("sort", new JObject(new JProperty("fieldName", "fieldValue")))
            );

            var actual = stubValue.SetupSorting("fieldName", "fieldValue");

            actual.Should().BeEquivalentTo(expected);
        }
        [Fact]
        public void SetupAggregation_EmptyFieldNameList()
        {
            var expected = new JObject();

            var aggregationFieldList = Array.Empty<AggregationField>();

            var actual = new JObject().WithAggregation(aggregationFieldList);

            actual.Should().BeEquivalentTo(expected);
        }
        [Fact]
        public void SetupAggregation_OneField_Success()
        {
            var expected = JObject.Parse("{\"aggs\":{\"NodeType\":{\"terms\":{\"field\":\"NodeTypeAggregate\", \"missing\":\"__hasNoValue\",\"size\": 100}}}}");

            var aggregationFieldList = new[]
            {
                new AggregationField("NodeType", string.Empty, "NodeTypeAggregate")
            };

            var actual = new JObject().WithAggregation(aggregationFieldList);

            actual.Should().BeEquivalentTo(expected);
        }
        [Fact]
        public void SetupAggregation_TwoField_Success()
        {
            var expected = JObject.Parse("{\"aggs\":{\"NodeType\":{\"terms\":{\"field\":\"NodeTypeAggregate\",\"missing\":\"__hasNoValue\", \"size\": 100}}, \"NodeName\":{\"terms\":{\"field\":\"NodeNameAggregate\", \"missing\":\"__hasNoValue\", \"size\": 100}}}}");

            var aggregationFieldList = new[]
            {
                new AggregationField("NodeType", string.Empty, "NodeTypeAggregate"),
                new AggregationField("NodeName", string.Empty, "NodeNameAggregate")
            };

            var actual = new JObject().WithAggregation(aggregationFieldList);

            actual.Should().BeEquivalentTo(expected);
        }
        [Fact]
        public void SetupAggregation_OneAlias_Success()
        {
            var expected = JObject.Parse("{\"aggs\":{\"NodeAlias\":{\"terms\":{\"field\":\"NodeTypeAggregate\", \"missing\":\"__hasNoValue\",\"size\": 100}}}}");

            var aggregationFieldList = new[]
            {
                new AggregationField("NodeType", "NodeAlias", "NodeTypeAggregate")
            };

            var actual = new JObject().WithAggregation(aggregationFieldList);

            actual.Should().BeEquivalentTo(expected);
        }
    }
}