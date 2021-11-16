using AutoFixture.Xunit2;
using AutoFixture;
using FluentAssertions;
using Iis.Elastic.SearchQueryExtensions;
using Iis.Interfaces.Elastic;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Iis.UnitTests.Iis.Elastic.Tests.Helpers;

namespace Iis.UnitTests.Iis.Elastic.Tests.SearchParamsContextTests
{
    public class CreateFromTests
    {
        [Theory]
        [AutoData]
        public void CreateFrom_WhenElasticMultiSearchParamsIsNull_ShouldThrowArgumentNullException(Dictionary<string, string> queries)
        {
            Func<ISearchParamsContext> func = () => SearchParamsContext.CreateFrom(null, queries);

            func.Should().Throw<ArgumentNullException>();
        }

        [Theory]
        [AutoMoqData]
        public void CreateFrom_WhenSearchParamsAreEmpty_ShouldThrowArgumentException(
            Mock<IElasticMultiSearchParams> searchParamsMock,
            Dictionary<string, string> queries)
        {
            searchParamsMock.SetupProperty(_ => _.SearchParams, new List<SearchParameter>());
            Func<ISearchParamsContext> func = () => SearchParamsContext.CreateFrom(null, queries);

            func.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void CreateFrom_ShouldCreateContext()
        {
            var fixture = ElasticMultiSearchParamsFixture.CreateFixture();
            var searchParams = fixture.Create<IElasticMultiSearchParams>();
            var queries = fixture.Create<Dictionary<string, string>>();
            var expectedBaseSearchParameter = searchParams.SearchParams.First();
            var expectedHistorySearchParameter = searchParams.SearchParams.Skip(1).FirstOrDefault();

            var context = SearchParamsContext.CreateFrom(searchParams, queries);

            context.ElasticMultiSearchParams.Should().BeEquivalentTo(searchParams);
            context.BaseSearchParameter.Should().BeEquivalentTo(expectedBaseSearchParameter);
            context.HistorySearchParameter.Should().BeEquivalentTo(expectedHistorySearchParameter);
            context.AggregateHistoryResultQueries.Should().BeEquivalentTo(queries);
        }
    }
}