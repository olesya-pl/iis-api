using AutoFixture;
using FluentAssertions;
using Iis.Elastic.SearchQueryExtensions;
using Iis.Interfaces.Elastic;
using System.Collections.Generic;
using Xunit;
using Iis.UnitTests.Iis.Elastic.Tests.Helpers;
using Iis.Domain.Elastic;
using Newtonsoft.Json.Linq;

namespace Iis.UnitTests.Iis.Elastic.Tests.SearchParamsContextTests
{
    public class GetAdditionalQueriesForAggregateTests
    {
        [Fact]
        public void GetAdditionalQueriesForAggregate_WhenAggregationFieldIsNull_ShouldReturnFullHistoryQuery()
        {
            JArray expected = JToken.Parse(@"[
        {
          ""query_string"": {
            ""query"": ""\""85aafaebb57d4a5f86dbef6b061fa601 b1a932cf17644578a821b483918e3ef8 ed83ee39abd2431aaf246c0c6c391857\"" OR 85aafaebb57d4a5f86dbef6b061fa601 b1a932cf17644578a821b483918e3ef8 ed83ee39abd2431aaf246c0c6c391857~"",
            ""fuzziness"": 0,
            ""boost"": 0.05,
            ""lenient"": true,
            ""fields"": [
              ""Id""
            ]
          }
        }
      ]") as JArray;
            var context = InitContext();

            var actual = context.GetAdditionalQueriesForAggregate();

            actual.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void GetAdditionalQueriesForAggregate_WhenAggregationFieldIsNotIncludedInHistoryResultQueries_ShouldReturnFullHistoryQuery()
        {
            JArray expected = JToken.Parse(@"[
        {
          ""query_string"": {
            ""query"": ""\""85aafaebb57d4a5f86dbef6b061fa601 b1a932cf17644578a821b483918e3ef8 ed83ee39abd2431aaf246c0c6c391857\"" OR 85aafaebb57d4a5f86dbef6b061fa601 b1a932cf17644578a821b483918e3ef8 ed83ee39abd2431aaf246c0c6c391857~"",
            ""fuzziness"": 0,
            ""boost"": 0.05,
            ""lenient"": true,
            ""fields"": [
              ""Id""
            ]
          }
        }
      ]") as JArray;
            var aggregationField = new AggregationField("test.nameAggregate", "Test", "test.nameAggregate", null);
            var context = InitContext();

            var actual = context.GetAdditionalQueriesForAggregate(aggregationField);

            actual.Should().BeEquivalentTo(expected);
        }

        [Theory]
        [MemberData(nameof(GetAggregationFields))]
        public void GetAdditionalQueriesForAggregate_WhenAggregationFieldIsIncludedInHistoryResultQueries_ShouldReturnPartialHistoryQuery(
            AggregationField aggregationField,
            string expectedJson)
        {
            JArray expected = JToken.Parse(expectedJson) as JArray;
            var context = InitContext();

            var actual = context.GetAdditionalQueriesForAggregate(aggregationField);

            actual.Should().BeEquivalentTo(expected);
        }

        private static ISearchParamsContext InitContext()
        {
            var fixture = ElasticMultiSearchParamsFixture.CreateFixture();
            var queryFields = fixture.Create<List<IIisElasticField>>();
            var searchParams = new ElasticMultiSearchParams
            {
                From = 1,
                Size = 10,
                SearchParams = new List<(string Query, List<IIisElasticField> Fields)>
                {
                    ("(\"тестовий\" OR тестовий~)", queryFields),
                    ("85aafaebb57d4a5f86dbef6b061fa601 b1a932cf17644578a821b483918e3ef8 ed83ee39abd2431aaf246c0c6c391857", new List<IIisElasticField>{ new IisElasticField { Name = "Id", Boost = 0.05m } })
                }
            };
            var aggregateHistoryResultQueries = new Dictionary<string, string>
            {
                { "importance.nameAggregate", "85aafaebb57d4a5f86dbef6b061fa601 b1a932cf17644578a821b483918e3ef8" },
                { "Приналежність", "ed83ee39abd2431aaf246c0c6c391857" }
            };
            var context = SearchParamsContext.CreateFrom(searchParams, aggregateHistoryResultQueries ?? new Dictionary<string, string>());

            return context;
        }

        public static IEnumerable<object[]> GetAggregationFields()
        {
            yield return new object[]
            {
                new AggregationField("importance.nameAggregate", "Важливість", "importance.nameAggregate", null),
                @"[
        {
          ""query_string"": {
            ""query"": ""\""85aafaebb57d4a5f86dbef6b061fa601 b1a932cf17644578a821b483918e3ef8\"" OR 85aafaebb57d4a5f86dbef6b061fa601 b1a932cf17644578a821b483918e3ef8~"",
            ""fuzziness"": 0,
            ""boost"": 0.05,
            ""lenient"": true,
            ""fields"": [
              ""Id""
            ]
          }
        }
      ]"
            };
            yield return new object[]
            {
                new AggregationField("affiliation.nameAggregate", "Приналежність", "affiliation.nameAggregate", null),
                @"[
        {
          ""query_string"": {
            ""query"": ""\""ed83ee39abd2431aaf246c0c6c391857\"" OR ed83ee39abd2431aaf246c0c6c391857~"",
            ""fuzziness"": 0,
            ""boost"": 0.05,
            ""lenient"": true,
            ""fields"": [
              ""Id""
            ]
          }
        }
      ]"
            };
        }
    }
}