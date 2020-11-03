using System.Collections.Generic;
using System.Linq;
using Iis.DataModel.Elastic;
using Iis.DbLayer.Elastic;
using Iis.Interfaces.Ontology.Schema;
using Iis.OntologySchema.DataTypes;
using Moq;
using Xunit;

namespace Iis.UnitTests.Iis.Elastic.Tests
{
    public class ElasticConfigurationTests
    {
        private static IisElasticConfiguration CreateSut(string typeName, List<AttributeInfoItem> ontologyFields)
        {
            var nodeTypeMock = new Mock<INodeTypeLinked>();

            var attributeInfoListMock = new Mock<IAttributeInfoList>();
            attributeInfoListMock.Setup(p => p.Items).Returns(ontologyFields);

            var ontologySchemaMock = new Mock<IOntologySchema>();
            ontologySchemaMock.Setup(e => e.GetEntityTypeByName(typeName)).Returns(nodeTypeMock.Object);
            ontologySchemaMock.Setup(e => e.GetAttributesInfo(typeName)).Returns(attributeInfoListMock.Object);
            var sut = new IisElasticConfiguration(ontologySchemaMock.Object);
            return sut;
        }

        [Theory, RecursiveAutoData]
        public void GetOntologyIncludedFields_AllFieldsAreIncluded(string typeName,
            List<AttributeInfoItem> ontologyFields,
            List<ElasticFieldEntity> configuredFields)
        {
            //arrange
            ontologyFields[0] = new AttributeInfoItem(
                ontologyFields[0].DotName,
                ontologyFields[0].ScalarType,
                ontologyFields[0].AliasesList,
                true);
            configuredFields.First().Name = ontologyFields[0].DotName;

            foreach (var configuredField in configuredFields)
            {
                configuredField.IsExcluded = false;
                configuredField.TypeName = typeName;
            }

            var sut = CreateSut(typeName, ontologyFields);
            sut.ReloadFields(configuredFields);

            //act
            var res = sut.GetOntologyIncludedFields(new[] { typeName });

            //assert
            foreach (var ontologyField in ontologyFields)
            {
                var item = res.First(p => p.Name == ontologyField.DotName);
                Assert.Equal(ontologyField.IsAggregated, item.IsAggregated);
            }

            foreach (var configuredField in configuredFields)
            {
                var item = res.First(p => p.Name == configuredField.Name);
                Assert.Equal(configuredField.Fuzziness, item.Fuzziness);
                Assert.Equal(configuredField.Boost, item.Boost);
            }
        }        

        [Theory, RecursiveAutoData]
        public void GetOntologyIncludedFields_ConfiguredFieldsOfOtherType(string typeName,
            AttributeInfoItem ontologyField,
            ElasticFieldEntity configuredField)
        {
            //arrange
            configuredField.IsExcluded = false;

            var sut = CreateSut(typeName, new List<AttributeInfoItem> { ontologyField });
            sut.ReloadFields(new[] { configuredField });

            //act
            var res = sut.GetOntologyIncludedFields(new[] { typeName });

            //assert
            var item = res.FirstOrDefault(p => p.Name == configuredField.Name);
            Assert.Null(item);
        }

        [Theory, RecursiveAutoData]
        public void GetOntologyIncludedFields_ConfiguredFieldsOfOtherType_ButHasOntologyFieldWithTheSameName(string typeName,
            AttributeInfoItem ontologyField,
            ElasticFieldEntity configuredField)
        {
            //arrange
            ontologyField = new AttributeInfoItem(
                ontologyField.DotName, 
                ontologyField.ScalarType, 
                ontologyField.AliasesList, 
                true);
            configuredField.Name = ontologyField.DotName;
            configuredField.IsExcluded = false;

            var sut = CreateSut(typeName, new List<AttributeInfoItem> { ontologyField });
            sut.ReloadFields(new[] { configuredField });

            //act
            var res = sut.GetOntologyIncludedFields(new[] { typeName });

            //assert
            var item = res.First(p => p.Name == configuredField.Name);
            Assert.Equal(0, item.Fuzziness);
            Assert.Equal(1.0m, item.Boost);
        }

        [Theory, RecursiveAutoData]
        public void GetOntologyIncludedFields_IsExcluded_ButHasOntologyFieldWithTheSameName(string typeName,
            AttributeInfoItem ontologyField,
            ElasticFieldEntity configuredField)
        {
            //arrange
            ontologyField = new AttributeInfoItem(
                ontologyField.DotName,
                ontologyField.ScalarType,
                ontologyField.AliasesList,
                true);
            configuredField.Name = ontologyField.DotName;
            configuredField.TypeName = typeName;
            configuredField.IsExcluded = true;

            var sut = CreateSut(typeName, new List<AttributeInfoItem> { ontologyField });
            sut.ReloadFields(new[] { configuredField });

            //act
            var res = sut.GetOntologyIncludedFields(new[] { typeName });

            //assert
            var item = res.FirstOrDefault(p => p.Name == configuredField.Name);
            Assert.Null(item);
        }
    }
}
