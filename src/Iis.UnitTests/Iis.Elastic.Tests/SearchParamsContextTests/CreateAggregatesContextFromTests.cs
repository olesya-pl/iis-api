using AutoFixture;
using FluentAssertions;
using Iis.Elastic.SearchQueryExtensions;
using Iis.Interfaces.Elastic;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Iis.UnitTests.Iis.Elastic.Tests.Helpers;
using Iis.Domain.Elastic;

namespace Iis.UnitTests.Iis.Elastic.Tests.SearchParamsContextTests
{
    public class CreateAggregatesContextFromTests
    {
        [Theory]
        [MemberData(nameof(TestData.GetElasticFilterData), MemberType = typeof(TestData))]
        public void CreateAggregatesContextFrom_WhenBaseQueryIsExact_ShouldMakeAggregationsBaseQueryExact(
            ElasticFilter filter,
            string expectedAggregationBaseQuery,
            bool expectedIsQueryExact,
            bool expectedIsMatchAll)
        {
            var fixture = ElasticMultiSearchParamsFixture.CreateFixture();
            var queryFields = fixture.Create<List<IIisElasticField>>();
            var additionalQuery = string.Join(" ", fixture.Create<Guid[]>().Select(_ => _.ToString("N")));
            var additionalQueryFields = fixture.Create<List<IIisElasticField>>();
            var searchParams = new ElasticMultiSearchParams
            {
                From = filter.Offset,
                Size = filter.Limit,
                SearchParams = new List<SearchParameter>
                {
                    new SearchParameter(filter.ToQueryString(), queryFields, filter.IsExact),
                    new SearchParameter(additionalQuery, additionalQueryFields)
                }
            };
            var queries = fixture.Create<Dictionary<string, string>>();
            var context = SearchParamsContext.CreateFrom(searchParams, queries);

            var aggregationContext = SearchParamsContext.CreateAggregatesContextFrom(context, filter);

            aggregationContext.AggregateHistoryResultQueries.Should().BeEquivalentTo(queries);
            aggregationContext.IsBaseQueryExact.Should().Be(expectedIsQueryExact);
            aggregationContext.IsBaseQueryMatchAll.Should().Be(expectedIsMatchAll);
            aggregationContext.BaseSearchParameter.Should().NotBeNull();
            aggregationContext.BaseSearchParameter.Query.Should().Be(expectedAggregationBaseQuery);
            aggregationContext.BaseSearchParameter.Fields.Should().BeEquivalentTo(queryFields);
            aggregationContext.HistorySearchParameter.Should().NotBeNull();
            aggregationContext.HistorySearchParameter.Query.Should().Be(additionalQuery);
            aggregationContext.HistorySearchParameter.Fields.Should().BeEquivalentTo(additionalQueryFields);
            aggregationContext.HasAdditionalParameters.Should().BeTrue();
        }
    }
}