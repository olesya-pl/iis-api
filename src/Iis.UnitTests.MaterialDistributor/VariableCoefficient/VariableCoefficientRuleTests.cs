using Xunit;
using AutoFixture.Xunit2;
using FluentAssertions;
using Iis.MaterialDistributor.Contracts.Services;

namespace Iis.UnitTests.MaterialDistributor
{
    public class VariableCoefficientRuleTests
    {
        [Theory, AutoData]
        public void VariableCoefficientRule_GetCoefficientUsingFromCoefficientParam(VariableCoefficient fromCoef, VariableCoefficient toCoef)
        {
            var rule = new VariableCoefficientRule(fromCoef, toCoef);

            rule.Coefficient.Should().Be(fromCoef.Coefficient);
        }

        [Fact]
        public void VariableCoefficientRule_IsSatisfiedReturnsFalseForBothNullCoef()
        {
            var rule = new VariableCoefficientRule(null, null);

            rule.Coefficient.Should().Be(0);
            rule.IsSatisfied(1).Should().BeFalse();
        }

        [Theory]
        [InlineData(0, 5, 0, true)]
        [InlineData(0, 5, 1, true)]
        [InlineData(0, 5, 2, true)]
        [InlineData(0, 5, 3, true)]
        [InlineData(0, 5, 4, true)]
        [InlineData(0, 5, 5, false)]
        [InlineData(0, 5, 6, false)]
        [InlineData(5, 10, 4, false)]
        [InlineData(5, 10, 5, true)]
        [InlineData(5, 10, 9, true)]
        [InlineData(5, 10, 10, false)]
        [InlineData(5, 10, 11, false)]
        [InlineData(10, 15, 6, false)]
        [InlineData(10, 15, 10, true)]
        [InlineData(10, 15, 13, true)]
        [InlineData(10, 15, 15, false)]
        [InlineData(10, 15, 17, false)]
        public void VariableCoefficientRule_IsSatisfiedForDateDiff(int fromOffset, int toOffset, int dateDiff, bool expected)
        {
            var fromCoef = new VariableCoefficient(fromOffset, 100);
            var toCoef = new VariableCoefficient(toOffset, 50);

            var rule = new VariableCoefficientRule(fromCoef, toCoef);

            rule.IsSatisfied(dateDiff).Should().Be(expected);
        }

        [Theory]
        [InlineData(0, false)]
        [InlineData(14, false)]
        [InlineData(15, true)]
        [InlineData(16, false)]
        public void VariableCoefficientRule_IsSatisfiedForDateDiffAndNullToCoef(int dateDiff, bool expected)
        {
            var fromCoef = new VariableCoefficient(15, 10);

            var rule = new VariableCoefficientRule(fromCoef, null);

            rule.IsSatisfied(dateDiff).Should().Be(expected);
        }
    }
}