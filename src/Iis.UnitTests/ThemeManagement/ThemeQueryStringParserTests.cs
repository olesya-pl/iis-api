using System.Linq;
using FluentAssertions;
using Iis.Services;
using Xunit;

namespace Iis.UnitTests.ThemeManagement
{
    public class ThemeQueryStringParserTests
    {
        [Fact]
        public void ParseSuggesion()
        {
            //arrange
            var input = @"
{
    ""themeId"": null,
    ""suggestion"": ""assignee.username: olya olya olya AND ProcessedStatus.Title: \""Не оброблено\"" AND Source: «contour.doc» AND CreatedDate: [now-0d TO *]"",
    ""filteredItems"": [],
    ""searchByRelation"": [],
    ""searchByImageInput"": null
}
";
            //act
            var res = ThemeQueryParser.Parse(input);

            //assert
            res.Suggestion.Should().Be("assignee.username: olya olya olya AND ProcessedStatus.Title: \"Не оброблено\" AND Source: «contour.doc» AND CreatedDate: [now-0d TO *]");
        }

        [Fact]
        public void ParseSuggesionAndFilteredItems()
        {
            //arrange
            var input = @"
{
    ""types"": [],
    ""themeId"": null,
    ""suggestion"": ""бонд"",
    ""quickFilters"": { },
    ""filteredItems"": [
        {
            ""name"": ""Тип_ОР"",
            ""value"": [
                ""Військовий підрозділ""
            ]
        }
    ],
    ""selectedEntities"": []
}
";
            //act
            var res = ThemeQueryParser.Parse(input);

            //assert
            res.Suggestion.Should().Be("бонд");
            res.FilteredItems.Count.Should().Be(1);
            res.FilteredItems.First().Name.Should().Be("Тип_ОР");
            res.FilteredItems.First().Value.Should().Be("Військовий підрозділ");
        }

        [Fact]
        public void ParseSelectedEntities()
        {
            //arrange
            var input = @"
{
    ""types"": [],
    ""themeId"": null,
    ""suggestion"": null,
    ""quickFilters"": { },
    ""filteredItems"": [],
    ""selectedEntities"": [
        {
            ""id"": ""292b35cae998401da1e39e212142a2ed"",
            ""title"": ""Агутин"",
            ""typeName"": ""Person"",
            ""typeTitle"": ""Особа"",
            ""__typename"": ""AutocompleteEntityDto"",
            ""includeDescendants"": true
        }
    ]
}
";
            //act
            var res = ThemeQueryParser.Parse(input);

            //assert
            res.CherryPickedItems.Count.Should().Be(1);
            res.CherryPickedItems.First().IncludeDescendants.Should().BeTrue();
            res.CherryPickedItems.First().Item.Should().Be("292b35cae998401da1e39e212142a2ed");
        }
    }

    
}
