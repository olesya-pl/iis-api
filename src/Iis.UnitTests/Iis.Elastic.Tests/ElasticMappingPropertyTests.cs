using System;
using System.Linq;
using Newtonsoft.Json.Linq;
using FluentAssertions;
using FluentAssertions.Json;
using Xunit;
using Iis.Elastic;
using Iis.Elastic.Dictionaries;
using Iis.Elastic.ElasticMappingProperties;
namespace Iis.UnitTests.Iis.Elastic.Tests
{
    public class ElasticMappingPropertyTests
    {
        [Fact]
        public void DotnameConstructor_CreatesNestedProperties()
        {
            var metadata = KeywordProperty.Create("Metadata.features.PhoneNumber", false);
            Assert.Equal(ElasticMappingPropertyType.Nested, metadata.Type);
            Assert.Equal("Metadata", metadata.Name);
            var features = metadata.Properties.First(p => p.Name == "features");
            Assert.Equal(ElasticMappingPropertyType.Nested, features.Type);
            var phoneNumber = features.Properties.First(p => p.Name == "PhoneNumber");
            Assert.Equal(ElasticMappingPropertyType.Keyword, phoneNumber.Type);
        }

        [Fact]
        public void DotnameConstructor_CreatesNestedProperties_DenseVector()
        {
            var metadata = DenseVectorProperty.Create("Metadata.features.FaceVector", 128);
            Assert.Equal(ElasticMappingPropertyType.Nested, metadata.Type);
            Assert.Equal("Metadata", metadata.Name);
            var features = metadata.Properties.First(p => p.Name == "features");
            Assert.Equal(ElasticMappingPropertyType.Nested, features.Type);
            var phoneNumber = features.Properties.First(p => p.Name == "FaceVector");
            Assert.Equal(ElasticMappingPropertyType.DenseVector, phoneNumber.Type);
        }

        [Theory]
        [InlineData("propertyName", new string[]{"date_format_one","date_format_two"})]
        public void DateType_ShouldPropagateFormatInJObject(string propertyName, string[] formatArray)
        {
            var actual = DateProperty.Create(propertyName, formatArray).ToJObject();

            var expected = new JObject(
                new JProperty("type", "date"),
                new JProperty("format", string.Join("||", formatArray))
            );

            actual.Should().BeEquivalentTo(expected);
        }

        [Theory]
        [InlineData("propertyName")]
        public void DateType_IsOkIfNoFormatInJObject(string propertyName)
        {
            var actual = DateProperty.Create(propertyName, null).ToJObject();
            var expected = new JObject(
                new JProperty("type", "date")
            );
            actual.Should().BeEquivalentTo(expected);
        }

        [Theory]
        [InlineData("propertyName", TextTermVectorsEnum.WithPositionsOffsets)]
        public void TextType_ShouldHaveTermVector(string propertyName, TextTermVectorsEnum termVector)
        {
            var actual = TextProperty.Create(propertyName, termVector).ToJObject();

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
            var actual = TextProperty.Create(propertyName, TextTermVectorsEnum.No).ToJObject();

            var expected = new JObject(
                new JProperty("type", ElasticMappingPropertyType.Text.ToString().ToLower())
            );

            actual.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void TextType_ShouldHaveKeyword()
        {
            var actual = TextProperty.Create("propertyName", TextTermVectorsEnum.No, true).ToJObject();

            var expected =  JObject.Parse( 
            @"{
                'type' : 'text',
                'fields' : {
                    'keyword' : {
                        'type' : 'keyword',
                        'ignore_above' : 256
                    }
                }
            }");

            actual.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void TextType_NoTermVectorSpecified()
        {
            var actual = TextProperty.Create("propertyName", true).ToJObject();

            var expected =  JObject.Parse( 
            @"{
                'type' : 'text',
                'fields' : {
                    'keyword' : {
                        'type' : 'keyword',
                        'ignore_above' : 256
                    }
                }
            }");

            actual.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void ByteType_SingleNameProperty()
        {
            var actual = ByteProperty.Create("propertyName").ToJObject();

            var expected = JObject.Parse(
                @"{
                    'type':'byte'
                }"
            );

            actual.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void ByteProperty_ComplexNameProperty()
        {
            var actual = ByteProperty.Create("PropertyName.NestedPropertyName").ToJObject();

            var expected = JObject.Parse(
                @"{
                    'properties':{
                        'NestedPropertyName':{
                            'type':'byte'
                        }
                    }
                }"
            );

            actual.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void ByteProperty_EmptyPropertyNameNullRef()
        {
            Action act = () => ByteProperty.Create(string.Empty).ToJObject();

            act.Should()
                .Throw<System.InvalidOperationException>()
                .WithMessage("Sequence contains no elements");
        }

        [Fact]
        public void IntegerProperty_SingleNameProperty()
        {
            var actual = IntegerProperty.Create("propertyName").ToJObject();

            var expected = JObject.Parse(
                @"{
                    'type':'integer'
                }"
            );

            actual.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void IntegerProperty_ComplexNameProperty()
        {
            var actual = IntegerProperty.Create("PropertyName.NestedPropertyName").ToJObject();

            var expected = JObject.Parse(
                @"{
                    'properties':{
                        'NestedPropertyName':{
                            'type':'integer'
                        }
                    }
                }"
            );

            actual.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void KeyWordProperty_SingleNameProperty()
        {
            var actual = KeywordProperty.Create("propertyName", false).ToJObject();

            var expected = JObject.Parse(
                @"{
                    'type':'keyword'
                }"
            );

            actual.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void KeyWordProperty_SingleNamePropertySupportsNull()
        {
            var actual = KeywordProperty.Create("propertyName", true).ToJObject();

            var expected = JObject.Parse(
                @"{
                    'type':'keyword',
                    'null_value':'NULL'
                }"
            );

            actual.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void KeyWordProperty_ComplexNamePropertySupportsNull()
        {
            var actual = KeywordProperty.Create("PropertyName.NestedPropertyName", true).ToJObject();

            var expected = JObject.Parse(
                @"{
                    'properties':{
                        'NestedPropertyName':{
                            'type':'keyword',
                            'null_value':'NULL'
                        }
                    }
                }"
            );

            actual.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void AliasProprty_Property()
        {
            var actual = AliasProperty.Create("propertyName", "user.userName");

            var expected = JObject.Parse(
                @"{
                    'type':'alias',
                    'path':'user.userName'
                }
                "
            );

            actual.ToJObject().Should().BeEquivalentTo(expected);

            actual.Type.Should().Be(ElasticMappingPropertyType.Alias);
        }

        [Fact]
        public void DateRangeProperty_SinglePropertyWithEmptyFormat()
        {
            var actual = DateRangeProperty.Create("propertyName").ToJObject();

            var expected = JObject.Parse(
                @"{
                    'type':'date_range'
                }
                "
            );

            actual.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void DateRangeProperty_ComplexPropertyWithEmptyFormat()
        {
            var actual = DateRangeProperty.Create("PropertyName.NestedPropertyName").ToJObject();

            var expected = JObject.Parse(
                @"{
                    'properties':{
                        'NestedPropertyName':{
                            'type':'date_range'
                        }
                    }
                }"
            );

            actual.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void DateRangeProperty_ComplexPropertyWithFormat()
        {
            var actual = DateRangeProperty.Create("PropertyName.NestedPropertyName", new string[]{"date_format_one","date_format_two"}).ToJObject();

            var expected = JObject.Parse(
                @"{
                    'properties':{
                        'NestedPropertyName':{
                            'type':'date_range',
                            'format':'date_format_one||date_format_two'
                        }
                    }
                }"
            );

            actual.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void FloatRangeProperty_SingleNameProperty()
        {
            var actual = FloatRangeProperty.Create("propertyName").ToJObject();

            var expected = JObject.Parse(
                @"{
                    'type':'float_range'
                }"
            );

            actual.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void FloatRangeProperty_ComplexNameProperty()
        {
            var actual = FloatRangeProperty.Create("PropertyName.NestedPropertyName").ToJObject();

            var expected = JObject.Parse(
                @"{
                    'properties':{
                        'NestedPropertyName':{
                            'type':'float_range'
                        }
                    }
                }"
            );

            actual.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void IntegerRangeProperty_SingleNameProperty()
        {
            var actual = IntegerRangeProperty.Create("propertyName").ToJObject();

            var expected = JObject.Parse(
                @"{
                    'type':'integer_range'
                }"
            );

            actual.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void IntegerRangeProperty_ComplexNameProperty()
        {
            var actual = IntegerRangeProperty.Create("PropertyName.NestedPropertyName").ToJObject();

            var expected = JObject.Parse(
                @"{
                    'properties':{
                        'NestedPropertyName':{
                            'type':'integer_range'
                        }
                    }
                }"
            );

            actual.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void DenseVectorProperty_ComplexNameProperty()
        {
            var actual = DenseVectorProperty.Create("PropertyName.NestedPropertyName", 1).ToJObject();

            var expected = JObject.Parse(
                @"{
                    'type': 'nested',
                    'properties':{
                        'NestedPropertyName':{
                            'type':'dense_vector',
                            'dims':1
                        }
                    }
                }"
            );

            actual.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void GeoPointProperty_SimpleNameProperty()
        {
            var actual = GeoPointProperty.Create("propertyName").ToJObject();

            var expected = JObject.Parse(
                @"{
                    'type':'geo_point'
                }"
            );

            actual.Should().BeEquivalentTo(expected);
        }
    }
}
