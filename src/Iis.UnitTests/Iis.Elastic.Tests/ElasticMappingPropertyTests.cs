using System.Linq;
using Newtonsoft.Json.Linq;
using FluentAssertions;
using FluentAssertions.Json;
using Iis.Elastic;
using Xunit;
using Newtonsoft.Json.Bson;

namespace Iis.UnitTests.Iis.Elastic.Tests
{
    public class ElasticMappingPropertyTests
    {
        [Fact]
        public void DotnameConstructor_CreatesNestedProperties()
        {
            var metadata = new ElasticMappingProperty("Metadata.features.PhoneNumber", ElasticMappingPropertyType.Keyword);
            Assert.Equal(ElasticMappingPropertyType.Nested, metadata.Type);
            Assert.Equal("Metadata", metadata.Name);
            var features = metadata.Properties.First(p => p.Name == "features");
            Assert.Equal(ElasticMappingPropertyType.Nested, features.Type);
            var phoneNumber = features.Properties.First(p => p.Name == "PhoneNumber");
            Assert.Equal(ElasticMappingPropertyType.Keyword, phoneNumber.Type);
        }
        
        [Theory]
        [InlineData("propertyName", new string[]{"date_format_one","date_format_two"})]
        public void DateType_ShouldPropagateFormatInJObject(string propertyName, string[] formatArray)
        {
            var dateType = ElasticMappingPropertyType.Date;

            var actual = new ElasticMappingProperty(propertyName, dateType, formats:formatArray).ToJObject();

            var expected = new JObject(
                new JProperty("type", dateType.ToString().ToLower()),
                new JProperty("format", string.Join("||", formatArray))
            );

            actual.Should().BeEquivalentTo(expected);
        }

        [Theory]
        [InlineData("propertyName")]
        public void DateType_IsOkIfNoFormatInJObject(string propertyName)
        {
            var dateType = ElasticMappingPropertyType.Date;

            var actual = new ElasticMappingProperty(propertyName, dateType).ToJObject();

            var expected = new JObject(
                new JProperty("type", dateType.ToString().ToLower())
            );

            actual.Should().BeEquivalentTo(expected);
        }

        [Theory]
        [InlineData("propertyName", new string[] { "date_format_one", "date_format_two" })]
        public void IntType_IsOkIfNoFormatInJObject(string propertyName, string[] formatArray)
        {
            var dateType = ElasticMappingPropertyType.Integer;

            var actual = new ElasticMappingProperty(propertyName, dateType, formats: formatArray).ToJObject();

            var expected = new JObject(
                new JProperty("type", dateType.ToString().ToLower())
            );

            actual.Should().BeEquivalentTo(expected);
        }
        
        [Theory]
        [InlineData("propertyName", ElasticMappingPropertyType.Text, "with_positions_offsets")]
        public void TextType_ShouldHaveTermVector(string propertyName, ElasticMappingPropertyType propertyType, string termVector)
        {
            var actual = new ElasticMappingProperty(propertyName, propertyType, termVector: termVector).ToJObject();

            var expected = new JObject(
                new JProperty("type", ElasticMappingPropertyType.Text.ToString().ToLower()),
                new JProperty("term_vector", "with_positions_offsets")
            );

            actual.Should().BeEquivalentTo(expected);
        }

        [Theory]
        [InlineData("propertyName", ElasticMappingPropertyType.Text)]
        public void TextType_ShouldNotHaveTermVector(string propertyName, ElasticMappingPropertyType propertyType)
        {
            var actual = new ElasticMappingProperty(propertyName, propertyType).ToJObject();

            var expected = new JObject(
                new JProperty("type", ElasticMappingPropertyType.Text.ToString().ToLower())
            );

            actual.Should().BeEquivalentTo(expected);
        }
    }
}
