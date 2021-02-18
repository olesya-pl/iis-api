using Xunit;
using Iis.Utility;
using FluentAssertions;
using System;

namespace Iis.UnitTests.Utiliity
{
    public class StringExtensionsTests
    {
        [Theory]
        [AutoMoqData]
        public void Capitalize(string input)
        {
            var res = input.Capitalize();
            char.IsUpper(res[0]).Should().BeTrue();
            input.Substring(1).Should().Equals(res.Substring(1));
        }

        [Fact]
        public void Capitalize_EmptyString()
        {
            var res = "".Capitalize();
            res.Should().Equals("");
        }

        [Theory]
        [AutoMoqData]
        public void ToLowerCamelCase(string input)
        {
            var res = input.ToLowerCamelCase();
            char.IsLower(res[0]).Should().BeTrue();
            input.Substring(1).Should().Equals(res.Substring(1));
        }

        [Fact]
        public void ToLowerCamelCase_EmptyString()
        {
            var res = "".Capitalize();
            res.Should().Equals("");
        }

        [Theory]
        [AutoMoqData]
        public void RemoveWhiteSpace(string value1, string value2)
        {
            var input = $" {value1}   {value2} ";
            var res = input.RemoveWhiteSpace();
            Assert.DoesNotContain(" ", res, StringComparison.Ordinal);
        }

        [Theory]
        [AutoMoqData]
        public void RemoveNewLines(string value1, string value2)
        {
            var input = $"\t{value1}\r\n{value2}{Environment.NewLine}";
            var res = input.RemoveNewLineCharacters();
            Assert.DoesNotContain("\r", res, StringComparison.Ordinal);
            Assert.DoesNotContain("\n", res, StringComparison.Ordinal);
            Assert.DoesNotContain("\t", res, StringComparison.Ordinal);
        }
    }
}
