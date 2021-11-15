using Iis.Interfaces.Elastic;
using System.Collections.Generic;

namespace Iis.UnitTests.Iis.Elastic.Tests.SearchParamsContextTests
{
    public static class TestData
    {
        public static IEnumerable<object[]> GetElasticFilterData()
        {
            yield return new object[]
            {
                new ElasticFilter()
                {
                    Suggestion = "омсбр"
                },
                "омсбр",
                false,
                false
            };
            yield return new object[]
            {
                new ElasticFilter()
                {
                    Suggestion = "(\"омсбр\" OR омсбр~)"
                },
                "(\"омсбр\" OR омсбр~)",
                true,
                false
            };
            yield return new object[]
            {
                new ElasticFilter()
                {
                    FilteredItems = new List<Property>(){}
                },
                "*",
                false,
                true
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
                "*",
                false,
                true
            };
            yield return new object[]
            {
                new ElasticFilter()
                {
                    Suggestion = "омсбр",
                    FilteredItems = new List<Property>()
                    {
                        new Property("Тип_ОР", "Особа"),
                        new Property("Чисельність", "Рота"),
                    }
                },
                "\"омсбр\" OR омсбр~",
                true,
                false
            };
            yield return new object[]
            {
                new ElasticFilter()
                {
                    CherryPickedItems = new List<CherryPickedItem>(){ new CherryPickedItem("some_id")}
                },
                "(Id:some_id OR parent.Id:some_id~0.95 OR bePartOf.Id:some_id~0.95)",
                true,
                false
            };
            yield return new object[]
            {
                new ElasticFilter()
                {
                    CherryPickedItems = new List<CherryPickedItem>(){ new CherryPickedItem("some_id", false)}
                },
                "(Id:some_id)",
                true,
                false
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
                "((\"омсбр\" OR омсбр~) OR (Id:some_id OR parent.Id:some_id~0.95 OR bePartOf.Id:some_id~0.95))",
                true,
                false
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
                "((\"омсбр\" OR омсбр~) OR (Id:some_id))",
                true,
                false
            };
        }
    }
}