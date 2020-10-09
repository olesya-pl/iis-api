using Iis.Elastic;
using Iis.Elastic.ElasticMappingProperties;
using Iis.Interfaces.Ontology.Schema;
using Iis.OntologySchema.DataTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace Iis.UnitTests.Iis.Elastic.Tests
{
    public class ElasticMappingConfigurationTests
    {
        [Fact]
        public void ConstructorFromAttributeInfo_Single()
        {
            var list = new List<AttributeInfoItem>
            {
                new AttributeInfoItem("name1", ScalarType.String, null),
            };
            var attributeInfo = new AttributeInfo("dummy", list);
            var result = new ElasticMappingConfiguration(attributeInfo);
            Assert.Single(result.Properties);
            var property = result.Properties[0];
            Assert.Equal("name1", property.Name);
            Assert.Equal(ElasticMappingPropertyType.Text, property.Type);
            Assert.Empty(property.Properties);
        }

        [Fact]
        public void ConstructorFromAttributeInfo_Single_IsFile()
        {
            var list = new List<AttributeInfoItem>
            {
                new AttributeInfoItem("name1", ScalarType.File, null),
            };
            var attributeInfo = new AttributeInfo("dummy", list);
            var result = new ElasticMappingConfiguration(attributeInfo);
            Assert.Single(result.Properties);
            var property = result.Properties[0];            
            Assert.Equal("name1", property.Name);
            Assert.Equal(ElasticMappingPropertyType.Nested, property.Type);
            Assert.Empty(property.Properties);
        }

        [Fact]
        public void ConstructorFromAttributeInfo_Single_IntegerRange()
        {
            var list = new List<AttributeInfoItem>
            {
                new AttributeInfoItem("name1", ScalarType.IntegerRange, null),
            };
            var attributeInfo = new AttributeInfo("dummy", list);
            var result = new ElasticMappingConfiguration(attributeInfo);
            Assert.Single(result.Properties);
            var property = result.Properties[0];
            Assert.Equal("name1", property.Name);
            Assert.Equal(ElasticMappingPropertyType.IntegerRange, property.Type);
            Assert.Empty(property.Properties);
        }

        [Fact]
        public void ConstructorFromAttributeInfo_Single_FloatRange()
        {
            var list = new List<AttributeInfoItem>
            {
                new AttributeInfoItem("name1", ScalarType.FloatRange, null),
            };
            var attributeInfo = new AttributeInfo("dummy", list);
            var result = new ElasticMappingConfiguration(attributeInfo);
            Assert.Single(result.Properties);
            var property = result.Properties[0];
            Assert.Equal("name1", property.Name);
            Assert.Equal(ElasticMappingPropertyType.FloatRange, property.Type);
            Assert.Empty(property.Properties);
        }

        [Fact]
        public void ConstructorFromAttributeInfo_Single_IsFile_JsonConversion()
        {
            var list = new List<AttributeInfoItem>
            {
                new AttributeInfoItem("name1", ScalarType.File, null),
            };
            var attributeInfo = new AttributeInfo("dummy", list);
            var result = new ElasticMappingConfiguration(attributeInfo);
            Assert.Single(result.Properties);
            var property = result.Properties[0];
            var json = property.ToJObject();
            Assert.Equal("nested", json["type"]);
        }

        [Fact]
        public void ConstructorFromAttributeInfo_Single_IntegerRange_JsonConversion()
        {
            var list = new List<AttributeInfoItem>
            {
                new AttributeInfoItem("name1", ScalarType.IntegerRange, null),
            };
            var attributeInfo = new AttributeInfo("dummy", list);
            var result = new ElasticMappingConfiguration(attributeInfo);
            Assert.Single(result.Properties);
            var property = result.Properties[0];
            var json = property.ToJObject();
            Assert.Equal("integer_range", json["type"]);
        }

        [Fact]
        public void ConstructorFromAttributeInfo_Single_FloatRange_JsonConversion()
        {
            var list = new List<AttributeInfoItem>
            {
                new AttributeInfoItem("name1", ScalarType.FloatRange, null),
            };
            var attributeInfo = new AttributeInfo("dummy", list);
            var result = new ElasticMappingConfiguration(attributeInfo);
            Assert.Single(result.Properties);
            var property = result.Properties[0];
            var json = property.ToJObject();
            Assert.Equal("float_range", json["type"]);
        }

        [Fact]
        public void ConstructorFromAttributeInfo_Deep2()
        {
            var list = new List<AttributeInfoItem>
            {
                new AttributeInfoItem("parent.child", ScalarType.String, null),
            };
            var attributeInfo = new AttributeInfo("dummy", list);
            var result = new ElasticMappingConfiguration(attributeInfo);
            Assert.Single(result.Properties);
            var parent = result.Properties[0];
            Assert.Equal("parent", parent.Name);
            Assert.Equal(ElasticMappingPropertyType.Nested, parent.Type);

            Assert.Single(parent.Properties);
            var child = parent.Properties[0];
            Assert.Equal("child", child.Name);
            Assert.Equal(ElasticMappingPropertyType.Text, child.Type);
        }

        [Fact]
        public void ConstructorFromAttributeInfo_Aliases()
        {
            var list = new List<AttributeInfoItem>
            {
                new AttributeInfoItem("parent.child", ScalarType.String, new List<string> { "alias1", "alias2" }),
            };
            var attributeInfo = new AttributeInfo("dummy", list);
            var result = new ElasticMappingConfiguration(attributeInfo);
            Assert.Equal(3, result.Properties.Count);
            var parent = result.Properties[0];
            Assert.Equal("parent", parent.Name);
            Assert.Equal(ElasticMappingPropertyType.Nested, parent.Type);

            Assert.Single(parent.Properties);
            var child = parent.Properties[0];
            Assert.Equal("child", child.Name);
            Assert.Equal(ElasticMappingPropertyType.Text, child.Type);

            var alias1 = result.Properties[1] as AliasProperty;
            Assert.Equal("alias1", alias1.Name);
            Assert.Equal(ElasticMappingPropertyType.Alias, alias1.Type);
            Assert.Equal("parent.child", alias1.Path);

            var alias2 = result.Properties[2] as AliasProperty;
            Assert.Equal("alias2", alias2.Name);
            Assert.Equal(ElasticMappingPropertyType.Alias, alias2.Type);
            Assert.Equal("parent.child", alias2.Path);
        }

        [Fact]
        public void ToMappingType_Test()
        {
            Assert.Equal(ElasticMappingPropertyType.Text, ElasticMappingConfiguration.ToMappingType(ScalarType.String));
            Assert.Equal(ElasticMappingPropertyType.Integer, ElasticMappingConfiguration.ToMappingType(ScalarType.Int));
            Assert.Equal(ElasticMappingPropertyType.Text, ElasticMappingConfiguration.ToMappingType(ScalarType.Decimal));
            Assert.Equal(ElasticMappingPropertyType.Date, ElasticMappingConfiguration.ToMappingType(ScalarType.Date));
            Assert.Equal(ElasticMappingPropertyType.Text, ElasticMappingConfiguration.ToMappingType(ScalarType.Boolean));
            Assert.Equal(ElasticMappingPropertyType.Text, ElasticMappingConfiguration.ToMappingType(ScalarType.Geo));
            Assert.Equal(ElasticMappingPropertyType.Nested, ElasticMappingConfiguration.ToMappingType(ScalarType.File));
            Assert.Equal(ElasticMappingPropertyType.Text, ElasticMappingConfiguration.ToMappingType(ScalarType.Json));
        }

        [Fact]
        public void ConvertToJson_Test()
        {
            const string MAPPING = "mappings";
            const string PROPERTIES = "properties";
            const string TYPE = "type";
            const string PATH = "path";

            var list = new List<AttributeInfoItem>
            {
                new AttributeInfoItem("part1.part2", ScalarType.String, new List<string> { "alias1", "alias2" }),
            };
            var attributeInfo = new AttributeInfo("dummy", list);
            var configuration = new ElasticMappingConfiguration(attributeInfo);
            var jConfig = configuration.ToJObject();
            var jProperties = jConfig[MAPPING];
            var jChildren = jProperties[PROPERTIES];
            Assert.Equal(3, jChildren.Children().Count());
            var jPart1 = jChildren["part1"];
            var jPart1Properties = jPart1[PROPERTIES];
            Assert.Single(jPart1Properties.Children());
            var jPart2 = jPart1Properties["part2"];
            Assert.Single(jPart2.Children());
            Assert.Equal("text", jPart2[TYPE]);

            var jAlias1 = jChildren["alias1"];
            Assert.Equal(2, jAlias1.Children().Count());
            Assert.Equal("alias", jAlias1[TYPE]);
            Assert.Equal("part1.part2", jAlias1[PATH]);

            var jAlias2 = jChildren["alias2"];
            Assert.Equal(2, jAlias2.Children().Count());
            Assert.Equal("alias", jAlias2[TYPE]);
            Assert.Equal("part1.part2", jAlias2[PATH]);
        }
    }
}
