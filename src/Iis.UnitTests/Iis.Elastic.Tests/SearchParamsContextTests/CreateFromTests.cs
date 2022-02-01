using System;
using System.Collections.Generic;
using System.Linq;
using AutoFixture;
using FluentAssertions;
using Iis.Elastic;
using Iis.Elastic.SearchQueryExtensions;
using Iis.Interfaces.Elastic;
using Iis.UnitTests.Iis.Elastic.Tests.Helpers;
using Xunit;

namespace Iis.UnitTests.Iis.Elastic.Tests.SearchParamsContextTests
{
    public class CreateFromTests
    {
        [Fact]
        public void CreateFrom_WhenElasticMultiSearchParamsIsNull_ShouldThrowArgumentNullException()
        {
            Func<ISearchParamsContext> func = () => SearchParamsContext.CreateFrom(null);

            func.Should().Throw<ArgumentNullException>();
        }

        [Theory]
        [AutoMoqData]
        public void CreateFrom_WhenSearchParamsAreEmpty_ShouldThrowArgumentException(
            ElasticMultiSearchParams searchParamsMock)
        {
            searchParamsMock.SearchParams = new List<SearchParameter>();
            Func<ISearchParamsContext> func = () => SearchParamsContext.CreateFrom(searchParamsMock);

            func.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void CreateFrom_ShouldCreateContext()
        {
            var fixture = ElasticMultiSearchParamsFixture.CreateFixture();
            var searchParams = fixture.Create<ElasticMultiSearchParams>();
            var expectedBaseSearchParameter = searchParams.SearchParams.First();

            var context = SearchParamsContext.CreateFrom(searchParams);

            context.ElasticMultiSearchParams.Should().BeEquivalentTo(searchParams);
            context.BaseSearchParameter.Should().BeEquivalentTo(expectedBaseSearchParameter);
        }
    }
}