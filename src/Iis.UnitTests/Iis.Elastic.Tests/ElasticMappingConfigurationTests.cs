using Iis.Elastic;
using Iis.Interfaces.Ontology.Schema;
using Iis.OntologySchema.DataTypes;
using System;
using System.Collections.Generic;
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
                new AttributeInfoItem("name1", ScalarType.String),
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
        public void ConstructorFromAttributeInfo_Deep2()
        {
            var list = new List<AttributeInfoItem>
            {
                new AttributeInfoItem("parent.child", ScalarType.String),
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
        public void ToMappingType_Test()
        {
            var mapping = new ElasticMappingConfiguration();
            Assert.Equal(ElasticMappingPropertyType.Text, mapping.ToMappingType(ScalarType.String));
            Assert.Equal(ElasticMappingPropertyType.Integer, mapping.ToMappingType(ScalarType.Int));
            Assert.Equal(ElasticMappingPropertyType.Text, mapping.ToMappingType(ScalarType.Decimal));
            Assert.Equal(ElasticMappingPropertyType.Date, mapping.ToMappingType(ScalarType.Date));
            Assert.Equal(ElasticMappingPropertyType.Text, mapping.ToMappingType(ScalarType.Boolean));
            Assert.Equal(ElasticMappingPropertyType.Text, mapping.ToMappingType(ScalarType.Geo));
            Assert.Equal(ElasticMappingPropertyType.Text, mapping.ToMappingType(ScalarType.File));
            Assert.Equal(ElasticMappingPropertyType.Text, mapping.ToMappingType(ScalarType.Json));
        }
    }
}
