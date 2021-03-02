using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AutoFixture.Xunit2;
using FluentAssertions;
using FluentAssertions.Json;
using Iis.Elastic;
using Iis.Elastic.ElasticMappingProperties;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Iis.UnitTests.Iis.Elastic.Tests
{
    public class ElasticMappingPropertyFactoryTests
    {
        [Theory, AutoData]
        public void MultipleNameParts_IsNestedProperty(string namePart1, string namePart2, 
            ElasticMappingPropertyType propertyType, 
            bool isAggregated)
        {
            var prop = ElasticMappingPropertyFactory.Create(new[] { namePart1, namePart2 }, propertyType, isAggregated);
            Assert.Equal(ElasticMappingPropertyType.Nested, prop.First().Type);
            Assert.Equal(namePart1, prop.First().Name);
        }

        [Theory, AutoData]
        public void TextProperty_IsNotAggregared_ContainsSingleProperty(string namePart1)
        {
            var prop = ElasticMappingPropertyFactory.Create(new[] { namePart1}, ElasticMappingPropertyType.Text, false);
            Assert.Single(prop);
            Assert.Equal(ElasticMappingPropertyType.Text, prop.First().Type);
            Assert.Equal(namePart1, prop.First().Name);
        }

        [Theory, AutoData]
        public void TextProperty_IsAggregared_ContainsTwoProperties(string namePart1)
        {
            var prop = ElasticMappingPropertyFactory.Create(new[] { namePart1 }, ElasticMappingPropertyType.Text, true);
            Assert.Equal(2, prop.Count());
            Assert.Contains(prop, p => p.Type == ElasticMappingPropertyType.Text && p.Name == namePart1);
            Assert.Contains(prop, p => p.Type == ElasticMappingPropertyType.Keyword 
                && p.Name == $"{namePart1}Aggregate");
        }

        [Theory, AutoData]
        public void IntegerProperty(string namePart1)
        {
            var prop = ElasticMappingPropertyFactory.Create(new[] { namePart1 }, ElasticMappingPropertyType.Integer, false);
            Assert.Single(prop);
            Assert.Equal(ElasticMappingPropertyType.Integer, prop.First().Type);
            Assert.Equal(namePart1, prop.First().Name);
        }

        [Theory, AutoData]
        public void IntegerRangeProperty(string namePart1)
        {
            var prop = ElasticMappingPropertyFactory.Create(new[] { namePart1 }, ElasticMappingPropertyType.IntegerRange, false);
            Assert.Single(prop);
            Assert.Equal(ElasticMappingPropertyType.IntegerRange, prop.First().Type);
            Assert.Equal(namePart1, prop.First().Name);
        }

        [Theory, AutoData]
        public void KeywordProprty_DoesNotSupport_NullValue(string namePart1)
        {
            var prop = ElasticMappingPropertyFactory.Create(new[] { namePart1 }, ElasticMappingPropertyType.Keyword, false);
            Assert.Single(prop);
            Assert.Equal(ElasticMappingPropertyType.Keyword, prop.First().Type);
            Assert.Equal(namePart1, prop.First().Name);

            var actual = prop.First().ToJObject();
            var expected = JObject.Parse(
                @"{
                    'type':'keyword'
                }"
            );
            actual.Should().BeEquivalentTo(expected);
        }
    }
}
