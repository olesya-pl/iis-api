using System.Collections.Generic;
using FluentAssertions;
using Iis.Elastic.SearchQueryExtensions;
using Iis.Interfaces.Elastic;
using Xunit;

namespace Iis.UnitTests.Iis.Elastic.Tests
{
    public class SearchQueryExtensionToQueryStringTests
    {
        public static IEnumerable<object[]> GetElasticFilterData()
        {
            yield return new object[]
            {
                new ElasticFilter()
                {
                    Suggestion = "омсбр"
                },
                "омсбр"
            };
            yield return new object[]
            {
                new ElasticFilter()
                {
                    FilteredItems = new List<Property>()
                    {
                        new Property("Тип_ОР", "Особа"),
                        new Property("Чисельність", "Рота")
                    }
                },
                "((Тип_ОР:\"Особа\") AND (Чисельність:\"Рота\"))"
            };
            yield return new object[]
            {
                new ElasticFilter()
                {
                    FilteredItems = new List<Property>()
                    {
                        new Property("Тип_ОР", "Особа"),
                        new Property("Тип_ОР", "Танк")
                    }
                },
                "((Тип_ОР:\"Особа\" OR Тип_ОР:\"Танк\"))"
            };
            yield return new object[]
            {
                new ElasticFilter()
                {
                    FilteredItems = new List<Property>()
                    {
                        new Property("Тип_ОР", "Особа"),
                        new Property("Тип_ОР", "Танк"),
                        new Property("Чисельність", "Рота"),
                        new Property("Чисельність", "Дивізія"),
                    }
                },
                "((Тип_ОР:\"Особа\" OR Тип_ОР:\"Танк\") AND (Чисельність:\"Рота\" OR Чисельність:\"Дивізія\"))"
            };
            yield return new object[]
            {
                new ElasticFilter()
                {
                    Suggestion = "омсбр",
                    FilteredItems = new List<Property>()
                    {
                        new Property("Тип_ОР", "Особа"),
                        new Property("Тип_ОР", "Танк"),
                        new Property("Чисельність", "Рота"),
                        new Property("Чисельність", "Дивізія"),
                    }
                },
                "((омсбр) AND ((Тип_ОР:\"Особа\" OR Тип_ОР:\"Танк\") AND (Чисельність:\"Рота\" OR Чисельність:\"Дивізія\")))"
            };
            yield return new object[]
            {
                new ElasticFilter()
                {
                    CherryPickedItems = new List<CherryPickedItem>(){ new CherryPickedItem("some_id")}
                },
                "(Id:some_id OR parent.Id:some_id~0.95 OR bePartOf.Id:some_id~0.95)"
            };
            yield return new object[]
            {
                new ElasticFilter()
                {
                    CherryPickedItems = new List<CherryPickedItem>(){ new CherryPickedItem("some_id", false)}
                },
                "(Id:some_id)"
            };
            yield return new object[]
            {
                new ElasticFilter()
                {
                    Suggestion = "омсбр",
                    FilteredItems = new List<Property>()
                    {
                        new Property("Тип_ОР", "Особа"),
                        new Property("Тип_ОР", "Танк"),
                        new Property("Чисельність", "Рота"),
                        new Property("Чисельність", "Дивізія"),
                    },
                    CherryPickedItems = new List<CherryPickedItem>(){ new CherryPickedItem("some_id")}
                },
                "(((омсбр) AND ((Тип_ОР:\"Особа\" OR Тип_ОР:\"Танк\") AND (Чисельність:\"Рота\" OR Чисельність:\"Дивізія\"))) OR (Id:some_id OR parent.Id:some_id~0.95 OR bePartOf.Id:some_id~0.95))"
            };
            yield return new object[]
            {
                new ElasticFilter()
                {
                    Suggestion = "омсбр",
                    FilteredItems = new List<Property>()
                    {
                        new Property("Тип_ОР", "Особа"),
                        new Property("Тип_ОР", "Танк"),
                        new Property("Чисельність", "Рота"),
                        new Property("Чисельність", "Дивізія"),
                    },
                    CherryPickedItems = new List<CherryPickedItem>(){ new CherryPickedItem("some_id", false)}
                },
                "(((омсбр) AND ((Тип_ОР:\"Особа\" OR Тип_ОР:\"Танк\") AND (Чисельність:\"Рота\" OR Чисельність:\"Дивізія\"))) OR (Id:some_id))"
            };
        }

        [Theory]
        [MemberData(nameof(GetElasticFilterData))]
        public void ToQueryString_ShouldReturnValidQuery(ElasticFilter filter, string expected)
        {
            var actual = filter.ToQueryString();

            actual.Should().Be(expected);
        }
    }
}