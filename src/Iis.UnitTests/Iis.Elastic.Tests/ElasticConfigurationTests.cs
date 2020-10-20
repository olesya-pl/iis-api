using System.Collections.Generic;
using System.Linq;
using Iis.DataModel.Elastic;
using Iis.DbLayer.Elastic;
using Iis.Interfaces.Ontology.Schema;
using Moq;
using Xunit;

namespace Iis.UnitTests.Iis.Elastic.Tests
{
    public class ElasticConfigurationTests
    {
        [Theory, RecursiveAutoData]
        public void GetOntologyIncludedFields_AllFieldsAreIncluded(string typeName,
            List<NodeAggregationInfo> ontologyFields,
            List<ElasticFieldEntity> configuredFields)
        {
            //arrange
            ontologyFields.First().IsAggregated = true;
            configuredFields.First().Name = ontologyFields.First().Name;

            foreach (var configuredField in configuredFields)
            {
                configuredField.IsExcluded = false;
                configuredField.TypeName = typeName;
            }
            
            var nodeTypeMock = new Mock<INodeTypeLinked>();
            nodeTypeMock.Setup(e => e.GetAttributeDotNamesRecursiveWithLimit(null, null, 0)).Returns(ontologyFields);
            var ontologySchemaMock = new Mock<IOntologySchema>();
            ontologySchemaMock.Setup(e => e.GetEntityTypeByName(typeName)).Returns(nodeTypeMock.Object);
            var sut = new IisElasticConfiguration(ontologySchemaMock.Object);
            sut.ReloadFields(configuredFields);

            //act
            var res = sut.GetOntologyIncludedFields(new[] { typeName });

            //assert
            foreach (var ontologyField in ontologyFields)
            {
                var item = res.First(p => p.Name == ontologyField.Name);
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
            NodeAggregationInfo ontologyField,
            ElasticFieldEntity configuredField)
        {
            //arrange
            configuredField.IsExcluded = false;

            var nodeTypeMock = new Mock<INodeTypeLinked>();
            nodeTypeMock.Setup(e => e.GetAttributeDotNamesRecursiveWithLimit(null, null, 0))
                .Returns(new List<NodeAggregationInfo> { ontologyField });
            var ontologySchemaMock = new Mock<IOntologySchema>();
            ontologySchemaMock.Setup(e => e.GetEntityTypeByName(typeName)).Returns(nodeTypeMock.Object);
            var sut = new IisElasticConfiguration(ontologySchemaMock.Object);
            sut.ReloadFields(new[] { configuredField });

            //act
            var res = sut.GetOntologyIncludedFields(new[] { typeName });

            //assert
            var item = res.FirstOrDefault(p => p.Name == configuredField.Name);
            Assert.Null(item);
        }

        [Theory, RecursiveAutoData]
        public void GetOntologyIncludedFields_ConfiguredFieldsOfOtherType_ButHasOntologyFieldWithTheSameName(string typeName,
            NodeAggregationInfo ontologyField,
            ElasticFieldEntity configuredField)
        {
            //arrange
            ontologyField.IsAggregated = true;
            configuredField.Name = ontologyField.Name;
            configuredField.IsExcluded = false;

            var nodeTypeMock = new Mock<INodeTypeLinked>();
            nodeTypeMock.Setup(e => e.GetAttributeDotNamesRecursiveWithLimit(null, null, 0))
                .Returns(new List<NodeAggregationInfo> { ontologyField });
            var ontologySchemaMock = new Mock<IOntologySchema>();
            ontologySchemaMock.Setup(e => e.GetEntityTypeByName(typeName)).Returns(nodeTypeMock.Object);
            var sut = new IisElasticConfiguration(ontologySchemaMock.Object);
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
            NodeAggregationInfo ontologyField,
            ElasticFieldEntity configuredField)
        {
            //arrange
            ontologyField.IsAggregated = true;
            configuredField.Name = ontologyField.Name;
            configuredField.TypeName = typeName;
            configuredField.IsExcluded = true;

            var nodeTypeMock = new Mock<INodeTypeLinked>();
            nodeTypeMock.Setup(e => e.GetAttributeDotNamesRecursiveWithLimit(null, null, 0))
                .Returns(new List<NodeAggregationInfo> { ontologyField });
            var ontologySchemaMock = new Mock<IOntologySchema>();
            ontologySchemaMock.Setup(e => e.GetEntityTypeByName(typeName)).Returns(nodeTypeMock.Object);
            var sut = new IisElasticConfiguration(ontologySchemaMock.Object);
            sut.ReloadFields(new[] { configuredField });

            //act
            var res = sut.GetOntologyIncludedFields(new[] { typeName });

            //assert
            var item = res.FirstOrDefault(p => p.Name == configuredField.Name);
            Assert.Null(item);
        }
    }
}
