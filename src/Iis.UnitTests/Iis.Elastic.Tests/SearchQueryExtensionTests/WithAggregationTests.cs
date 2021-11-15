using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json.Linq;
using FluentAssertions;
using FluentAssertions.Json;
using Xunit;
using Iis.Elastic.SearchQueryExtensions;
using Moq;
using Iis.Interfaces.Elastic;

namespace Iis.UnitTests.Iis.Elastic.Tests.SearchQueryExtensionTests
{
    public class WithAggregationTests
    {
        private static string BasePathToJson = "Iis.Elastic.Tests/SearchQueryExtensionTestJson";

        [Theory]
        [AutoMoqData]
        public void WithAggregation_WithFilter_OnlyFilteredItemsAvailable(
            IMock<ISearchParamsContext> searchContextMock,
            IMock<IGroupedAggregationNameGenerator> aggregationNameGeneratorMock)
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

            var actual = new JObject().WithAggregation(aggregationFieldList, filter, aggregationNameGeneratorMock.Object, searchContextMock.Object);

            actual.Should().BeEquivalentTo(expected);
        }

        [Theory]
        [AutoMoqData]
        public void WithAggregation_WithFilter_WithSuggestion(
            IMock<ISearchParamsContext> searchContextMock,
            IMock<IGroupedAggregationNameGenerator> aggregationNameGeneratorMock)
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

            var actual = new JObject().WithAggregation(aggregationFieldList, filter, aggregationNameGeneratorMock.Object, searchContextMock.Object);

            actual.Should().BeEquivalentTo(expected);
        }

        [Theory]
        [AutoMoqData]
        public void WithAggregation_WithFilter_WithOneFilteredItems(
            IMock<ISearchParamsContext> searchContextMock,
            IMock<IGroupedAggregationNameGenerator> aggregationNameGeneratorMock)
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

            var actual = new JObject().WithAggregation(aggregationFieldList, filter, aggregationNameGeneratorMock.Object, searchContextMock.Object);

            actual.Should().BeEquivalentTo(expected);
        }

        private JObject GetActualJObject(string name)
        {
            var path = $"{BasePathToJson}/{name.Replace("_", "")}.json";
            if (!File.Exists(path))
                throw new FileNotFoundException($"file was not found by path {path}");

            var jsonContent = File.ReadAllText(path);

            return JObject.Parse(jsonContent);
        }
    }
}