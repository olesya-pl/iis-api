using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json.Linq;
using FluentAssertions;
using FluentAssertions.Json;
using Xunit;
using Iis.Elastic.SearchQueryExtensions;
using Iis.Interfaces.Elastic;
using Moq;

namespace Iis.UnitTests.Iis.Elastic.Tests
{
    public class SearchQueryExtensionTests
    {
        private static string BasePathToJson = "Iis.Elastic.Tests/SearchQueryExtensionTestJson";
        [Theory]
        [InlineData(new[] {"*"}, 1, 5)]
        [InlineData(new[] {"Id", "Type", "Source"}, 0, 3)]
        public void WithSearchJson_Success(IEnumerable<string> resultFieldList, int from, int size)
        {
            var expected = JObject.Parse("{\"_source\":[" + string.Join(",", resultFieldList.Select(x => $"\"{x}\"")) +
                                         "], \"from\":" + from + ",\"size\":" + size + ",\"query\": {}}");

            var actual = SearchQueryExtension.WithSearchJson(resultFieldList, from, size);

            actual.Should().BeEquivalentTo(expected);
        }

        [Theory]
        [InlineData(1, 5)]
        public void WithSearchJson_DefaultResultFieldList(int from, int size)
        {
            var expected =
                JObject.Parse("{\"_source\":[\"*\"], \"from\":" + from + ",\"size\":" + size + ",\"query\": {}}");

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
            var expected =
                JObject.Parse(
                    "{\"_source\": [\"*\"], \"from\": 0, \"size\": 0,\"query\":{\"query_string\":{\"query\":\"" +
                    query + "\", \"lenient\":" + lenient.ToString().ToLower() + "}}}");

            var actual = new ExactQueryBuilder()
                .WithQueryString(query)
                .WithLeniency(lenient)
                .BuildSearchQuery();

            actual.Should().BeEquivalentTo(expected);
        }

        [Theory]
        [InlineData("fieldName:fieldValue")]
        public void SetupExactQuery_Success_NoLenient(string query)
        {
            var expected =
                JObject.Parse(
                    "{\"_source\": [\"*\"], \"from\": 0, \"size\": 0,\"query\":{\"query_string\":{\"query\":\"" +
                    query + "\"}}}");

            var actual = new ExactQueryBuilder()
                .WithQueryString(query)
                .BuildSearchQuery();

            actual.Should().BeEquivalentTo(expected);
        }

        [Theory]
        [InlineData("fieldName:fieldValue", 14, 88)]
        public void SetupExactQuery_Success_WithPagination(string query, int offset, int size)
        {
            var expected = JObject.Parse("{\"_source\": [\"*\"], \"from\": " + offset + ", \"size\": " + size +
                                         ",\"query\":{\"query_string\":{\"query\":\"" + query + "\"}}}");

            var actual = new ExactQueryBuilder()
                .WithQueryString(query)
                .WithPagination(offset, size)
                .BuildSearchQuery();

            actual.Should().BeEquivalentTo(expected);
        }

        [Theory]
        [InlineData("fieldName:fieldValue", 21, 69, "Id")]
        public void SetupExactQuery_Success_WithPagination_AndResultFields(string query, int offset, int size,
            string resultField)
        {
            var expected = JObject.Parse("{\"_source\": [\"" + resultField + "\"], \"from\": " + offset +
                                         ", \"size\": " + size + ",\"query\":{\"query_string\":{\"query\":\"" + query +
                                         "\"}}}");

            var actual = new ExactQueryBuilder()
                .WithQueryString(query)
                .WithPagination(offset, size)
                .WithResultFields(new[] {resultField})
                .BuildSearchQuery();

            actual.Should().BeEquivalentTo(expected);
        }

        [Theory]
        [InlineData("fieldName", "asc", "_last")]
        [InlineData("fieldName", "desc", "_first")]
        public void SetupSorting_Success(string sortColumn, string sortOrder, string missing)
        {
            var expected = JObject.Parse("{\"sort\":[{\"" + sortColumn + "\":{\"order\":\"" + sortOrder + "\", \"missing\":\""+missing+"\"}}]}");

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
            var expected =
                JObject.Parse(
                    "{\"sort\":[{\"fieldName1\":{\"order\":\"asc\", \"missing\":\"_last\"}}, {\"fieldName2\":{\"order\":\"desc\", \"missing\":\"_first\"}}]}");

            var stubValue = JObject.Parse("{\"sort\":[{\"fieldName1\":{\"order\":\"asc\", \"missing\":\"_last\"}}]}");

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
            var expected =
                JObject.Parse(
                    "{\"aggs\":{\"NodeType\":{\"terms\":{\"field\":\"NodeTypeAggregate\", \"missing\":\"__hasNoValue\",\"size\": 100}}}}");

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
            var expected = JObject.Parse(
                "{\"aggs\":{\"NodeType\":{\"terms\":{\"field\":\"NodeTypeAggregate\",\"missing\":\"__hasNoValue\", \"size\": 100}}, \"NodeName\":{\"terms\":{\"field\":\"NodeNameAggregate\", \"missing\":\"__hasNoValue\", \"size\": 100}}}}");

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
            var expected =
                JObject.Parse(
                    "{\"aggs\":{\"NodeAlias\":{\"terms\":{\"field\":\"NodeTypeAggregate\", \"missing\":\"__hasNoValue\",\"size\": 100}}}}");

            var aggregationFieldList = new[]
            {
                new AggregationField("NodeType", "NodeAlias", "NodeTypeAggregate")
            };

            var actual = new JObject().WithAggregation(aggregationFieldList);

            actual.Should().BeEquivalentTo(expected);
        }

        [Theory]
        [AutoMoqData]
        public void WithAggregation_WithFilter_OnlyFilteredItemsAvailable(IMock<ISearchParamsContext> searchContextMock)
        {
            var expected = GetActualJObject(nameof(WithAggregation_WithFilter_OnlyFilteredItemsAvailable));
            
            var aggregationFieldList = new[]
            {
                new AggregationField($"amount.name{SearchQueryExtension.AggregateSuffix}", "",
                    $"amount.name{SearchQueryExtension.AggregateSuffix}", "amount.name")
            };

            var filter = new ElasticFilter
            {
                FilteredItems = new List<Property>
                {
                    new Property {Name = $"amount.name{SearchQueryExtension.AggregateSuffix}", Value = "Батальйон"},
                    new Property {Name = $"classifiers.corps.name{SearchQueryExtension.AggregateSuffix}", Value = "ППО"}
                }
            };

            var actual = new JObject().WithAggregation(aggregationFieldList, filter, searchContextMock.Object);

            actual.Should().BeEquivalentTo(expected);
        }

        [Theory]
        [AutoMoqData]
        public void WithAggregation_WithFilter_WithSuggestion(IMock<ISearchParamsContext> searchContextMock)
        {
            var expected = GetActualJObject(nameof(WithAggregation_WithFilter_WithSuggestion));
            
            var aggregationFieldList = new[]
            {
                new AggregationField($"amount.name{SearchQueryExtension.AggregateSuffix}", "",
                    $"amount.name{SearchQueryExtension.AggregateSuffix}", "amount.name")
            };

            var filter = new ElasticFilter
            {
                Suggestion = "омсбр",
                FilteredItems = new List<Property>
                {
                    new Property {Name = $"amount.name{SearchQueryExtension.AggregateSuffix}", Value = "Батальйон"},
                    new Property {Name = $"classifiers.corps.name{SearchQueryExtension.AggregateSuffix}", Value = "ППО"}
                }
            };

            var actual = new JObject().WithAggregation(aggregationFieldList, filter, searchContextMock.Object);

            actual.Should().BeEquivalentTo(expected);
        }

        [Theory]
        [AutoMoqData]
        public void WithAggregation_WithFilter_WithOneFilteredItems(IMock<ISearchParamsContext> searchContextMock)
        {
            var expected = GetActualJObject(nameof(WithAggregation_WithFilter_WithOneFilteredItems));
            
            var aggregationFieldList = new[]
            {
                new AggregationField($"amount.name{SearchQueryExtension.AggregateSuffix}", "",
                    $"amount.name{SearchQueryExtension.AggregateSuffix}", "amount.name")
            };

            var filter = new ElasticFilter
            {
                FilteredItems = new List<Property>
                {
                    new Property {Name = $"amount.name{SearchQueryExtension.AggregateSuffix}", Value = "Батальйон"},
                }
            };

            var actual = new JObject().WithAggregation(aggregationFieldList, filter, searchContextMock.Object);

            actual.Should().BeEquivalentTo(expected);
        }

        private JObject GetActualJObject(string name)
        {
            var path = $"{BasePathToJson}/{name.Replace("_", "")}.json";
            if(!File.Exists(path))
                throw new FileNotFoundException($"file was not found by path {path}");

            var jsonContent = File.ReadAllText(path);
            
            return JObject.Parse(jsonContent);
        }
    }
}