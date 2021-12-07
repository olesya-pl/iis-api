using Xunit;
using System;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using FluentAssertions;
using FluentAssertions.Json;
using AutoFixture.Xunit2;
using Iis.Elastic.SearchQueryExtensions;

namespace Iis.UnitTests.Iis.Elastic.Tests
{
    public class GetByIdCollectionQueryBuilderTests
    {
        [Theory, AutoData]
        public void GetByIdCollection_NoFrom_No_Size(IReadOnlyCollection<Guid> idCollection)
        {
            var actual = new GetByIdCollectionQueryBuilder(idCollection)
                            .BuildSearchQuery();

            actual.Should()
                    .NotHaveElement("from")
                    .And
                    .HaveElement("size")
                    .And
                    .HaveElement("query");
        }

        [Theory, AutoData]
        public void GetByIdCollection_HasQueryWithIdCollection(IReadOnlyCollection<Guid> idCollection)
        {
            var strIdCollection = idCollection.Select(_ => $"\"{_.ToString("N")}\"").ToArray();

            var expected = JObject.Parse("{\"_source\":[\"*\"], \"size\":10000, \"query\":{\"ids\":{\"values\":[" + string.Join(',', strIdCollection) + "]}}}");

            var actual = new GetByIdCollectionQueryBuilder(idCollection)
                            .BuildSearchQuery();

            actual.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void GetByIdCollection_ExceptionWhenEmptyIdCollection()
        {
            Action act = () => new GetByIdCollectionQueryBuilder(Array.Empty<Guid>());

            act.Should()
                .Throw<ArgumentException>()
                .WithMessage("Parameter should not be null or empty. (Parameter 'idCollection')");
        }

        [Fact]
        public void GetByIdCollection_ExceptionWhenNullIdCollection()
        {
            Action act = () => new GetByIdCollectionQueryBuilder(null);

            act.Should()
                .Throw<ArgumentException>()
                .WithMessage("Parameter should not be null or empty. (Parameter 'idCollection')");
        }
    }
}