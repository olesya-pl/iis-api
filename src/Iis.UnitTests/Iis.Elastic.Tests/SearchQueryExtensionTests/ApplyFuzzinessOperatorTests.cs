using AutoFixture.Xunit2;
using FluentAssertions;
using Iis.Elastic;
using Iis.Elastic.SearchQueryExtensions;
using System.Linq;
using Xunit;

namespace Iis.UnitTests.Iis.Elastic.Tests.SearchQueryExtensionTests
{
    public class ApplyFuzzinessOperatorTests
    {
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void ApplyFuzzinessOperator_ShouldReturnEmptyString(string input)
        {
            var expected = string.Empty;

            var result = SearchQueryExtension.ApplyFuzzinessOperator(input);

            result.Should().Be(expected);
        }

        [Theory]
        [InlineData(SearchQueryExtension.Wildcard)]
        [InlineData("(\"омсбр\" OR омсбр~)")]
        public void ApplyFuzzinessOperator_ShouldReturnTheSame(string input)
        {
            var result = SearchQueryExtension.ApplyFuzzinessOperator(input);

            result.Should().Be(input);
        }

        [Theory]
        [AutoData]
        public void ApplyFuzzinessOperator_ShouldRemoveSymbols(string input)
        {
            var additional = string.Join(string.Empty, ElasticManager.RemoveSymbolsPattern);
            var inputWithSymbolsToRemove = string.Join(string.Empty, input, additional);
            var expected = $"\"{input}\" OR {input}~";

            var result = SearchQueryExtension.ApplyFuzzinessOperator(inputWithSymbolsToRemove);

            result.Should().Be(expected);
        }

        [Theory]
        [AutoData]
        public void ApplyFuzzinessOperator_ShouldEscapeSymbols(string input)
        {
            var additional = string.Join(string.Empty, ElasticManager.EscapeSymbolsPattern);
            var inputWithSymbolsToEscape = string.Join(string.Empty, input, additional);
            var escapedAdditionalSymbols = string.Join(string.Empty, ElasticManager.EscapeSymbolsPattern.Select(_ => $"\\{_}"));
            var escapedInput = string.Join(string.Empty, input, escapedAdditionalSymbols);
            var expected = $"\"{escapedInput}\" OR {escapedInput}~";

            var result = SearchQueryExtension.ApplyFuzzinessOperator(inputWithSymbolsToEscape);

            result.Should().Be(expected);
        }

        [Theory]
        [InlineData("\"input\"", "\"input\" OR \"input\"~")]
        [InlineData("\"input", "\"\"input\" OR \"input~")]
        [InlineData("input\"", "\"input\"\" OR input\"~")]
        [InlineData("input", "\"input\" OR input~")]
        public void ApplyFuzzinessOperator_ShouldApplyFuzziness(string input, string expected)
        {
            var result = SearchQueryExtension.ApplyFuzzinessOperator(input);

            result.Should().Be(expected);
        }
    }
}