using System;
using System.Collections.Generic;
using Xunit;
using AutoFixture.Xunit2;
using FluentAssertions;
using Iis.MaterialDistributor.Contracts.Services;

namespace Iis.UnitTests.MaterialDistributor
{
    public class VariableCoefficientRuleEvaluatorTests
    {
        [Fact]
        public void DefaultCoefficientWhenNullRuleCollection()
        {
            var coefficient = new VariableCoefficientRuleEvaluator()
                .GetVariableCoefficientValue(null, DateTime.Now, new MaterialDocument());

            coefficient.Should().Be(0);
        }

        [Fact]
        public void DefaultCoefficientWhenEmprtyRuleCollection()
        {
            var coefficient = new VariableCoefficientRuleEvaluator()
                .GetVariableCoefficientValue(Array.Empty<VariableCoefficientRule>(), DateTime.Now, new MaterialDocument());

            coefficient.Should().Be(0);
        }

        [Fact]
        public void DefaultCoefficientWhenNullDocument()
        {
            var ruleCollection = new List<VariableCoefficientRule>
            {
                new VariableCoefficientRule(null, null)
            };

            var coefficient = new VariableCoefficientRuleEvaluator()
                .GetVariableCoefficientValue(ruleCollection, DateTime.Now, null);

            coefficient.Should().Be(0);
        }

        [Theory]
        [InlineData(16, 14, 100)]
        [InlineData(16, 00, 100)]
        [InlineData(15, 15, 100)]
        [InlineData(15, 00, 100)]
        [InlineData(14, 35, 100)]
        [InlineData(13, 45, 100)]
        [InlineData(12, 15, 100)]
        [InlineData(11, 15, 75)]
        [InlineData(10, 25, 75)]
        [InlineData(09, 35, 75)]
        [InlineData(08, 45, 75)]
        [InlineData(07, 55, 75)]
        [InlineData(06, 15, 50)]
        [InlineData(05, 15, 0)]
        public void GetCoefficientByRegistrationDate(int hour, int minute, int expectedCoeficient)
        {
            var comparisonTimeStamp = new DateTime(2021, 01, 01, 16, 15, 25);

            var document = new MaterialDocument
            {
                Id = Guid.NewGuid(),
                RegistrationDate = new DateTime(2021, 01, 01, hour, minute, 25),
                CreatedDate = DateTime.UtcNow
            };

            var ruleCollection = new List<VariableCoefficientRule>
            {
                new VariableCoefficientRule(new VariableCoefficient(0, 100), new VariableCoefficient(5, 75)),
                new VariableCoefficientRule(new VariableCoefficient(5, 75), new VariableCoefficient(10, 50)),
                new VariableCoefficientRule(new VariableCoefficient(10, 50), null),
            };

            new VariableCoefficientRuleEvaluator().GetVariableCoefficientValue(ruleCollection, comparisonTimeStamp, document)
                .Should().Be(expectedCoeficient);
        }

        [Theory]
        [InlineData(16, 14, 100)]
        [InlineData(16, 00, 100)]
        [InlineData(15, 15, 100)]
        [InlineData(15, 00, 100)]
        [InlineData(14, 35, 100)]
        [InlineData(13, 45, 100)]
        [InlineData(12, 15, 100)]
        [InlineData(11, 15, 75)]
        [InlineData(10, 25, 75)]
        [InlineData(09, 35, 75)]
        [InlineData(08, 45, 75)]
        [InlineData(07, 55, 75)]
        [InlineData(06, 15, 50)]
        [InlineData(05, 15, 0)]
        public void GetCoefficientByCreatedDateAndRegistrationDateIsNull(int hour, int minute, int expectedCoeficient)
        {
            var comparisonTimeStamp = new DateTime(2021, 01, 01, 16, 15, 25);

            var document = new MaterialDocument
            {
                Id = Guid.NewGuid(),
                RegistrationDate = null,
                CreatedDate = new DateTime(2021, 01, 01, hour, minute, 25)
            };

            var ruleCollection = new List<VariableCoefficientRule>
            {
                new VariableCoefficientRule(new VariableCoefficient(0, 100), new VariableCoefficient(5, 75)),
                new VariableCoefficientRule(new VariableCoefficient(5, 75), new VariableCoefficient(10, 50)),
                new VariableCoefficientRule(new VariableCoefficient(10, 50), null),
            };

            new VariableCoefficientRuleEvaluator().GetVariableCoefficientValue(ruleCollection, comparisonTimeStamp, document)
                .Should().Be(expectedCoeficient);
        }
    }
}