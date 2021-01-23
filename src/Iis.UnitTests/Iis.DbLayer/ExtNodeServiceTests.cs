using System.Collections.Generic;

using AutoFixture.Xunit2;
using Iis.DataModel;
using Iis.DbLayer.Ontology.EntityFramework;
using Iis.DbLayer.Repositories;
using Iis.Interfaces.Ontology.Data;
using Iis.Interfaces.Ontology.Schema;
using Iis.OntologyData.DataTypes;
using Iis.Utility;

using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

using Moq;

using Xunit;

namespace Iis.UnitTests.Iis.DbLayer
{
    public class ExtNodeServiceTests
    {
        [Theory, AutoData]
        public void GetExtNode_IsAttribute_IntegerRangeType_SingleValue(int value) 
        {
            //arrange
            var attributeMock = new Mock<IAttribute>();
            attributeMock.Setup(e => e.Value).Returns(value.ToString());

            var attributeTypeMock = new Mock<IAttributeType>();
            attributeTypeMock.Setup(e => e.ScalarType).Returns(ScalarType.IntegerRange);
            
            var nodeTypeMock = new Mock<INodeTypeLinked>();
            nodeTypeMock.Setup(e => e.GetComputedRelationTypes()).Returns(new List<IRelationTypeLinked>());
            nodeTypeMock.Setup(e => e.AttributeType).Returns(attributeTypeMock.Object);
            
            var nodeMock = new Mock<INode>();
            nodeMock.Setup(e => e.Attribute).Returns(attributeMock.Object);
            nodeMock.Setup(e => e.NodeType).Returns(nodeTypeMock.Object);
            nodeMock.Setup(e => e.OutgoingRelations).Returns(new List<RelationData>());

            //act
            var sut = new ExtNodeService();
            var res = sut.GetExtNode(nodeMock.Object);

            //assert
            var gte = res.AttributeValue.GetType().GetProperty("gte").GetValue(res.AttributeValue, null);
            var lte = res.AttributeValue.GetType().GetProperty("lte").GetValue(res.AttributeValue, null);
            Assert.Equal(value.ToString(), gte.ToString());
            Assert.Equal(value.ToString(), lte.ToString());
        }

        [Theory, AutoData]
        public void GetExtNode_IsAttribute_IntegerRangeType_MultipleValues(int minValue, int maxValue)
        {
            //arrange
            var attributeMock = new Mock<IAttribute>();
            attributeMock.Setup(e => e.Value).Returns($"{minValue}-{maxValue}");

            var attributeTypeMock = new Mock<IAttributeType>();
            attributeTypeMock.Setup(e => e.ScalarType).Returns(ScalarType.IntegerRange);

            var nodeTypeMock = new Mock<INodeTypeLinked>();
            nodeTypeMock.Setup(e => e.GetComputedRelationTypes()).Returns(new List<IRelationTypeLinked>());
            nodeTypeMock.Setup(e => e.AttributeType).Returns(attributeTypeMock.Object);

            var nodeMock = new Mock<INode>();
            nodeMock.Setup(e => e.Attribute).Returns(attributeMock.Object);
            nodeMock.Setup(e => e.NodeType).Returns(nodeTypeMock.Object);
            nodeMock.Setup(e => e.OutgoingRelations).Returns(new List<RelationData>());

            //act
            var sut = new ExtNodeService();
            var res = sut.GetExtNode(nodeMock.Object);

            //assert
            var gte = res.AttributeValue.GetType().GetProperty("gte").GetValue(res.AttributeValue, null);
            var lte = res.AttributeValue.GetType().GetProperty("lte").GetValue(res.AttributeValue, null);
            Assert.Equal(minValue.ToString(), gte.ToString());
            Assert.Equal(maxValue.ToString(), lte.ToString());
        }

        [Theory, AutoData]
        public void GetExtNode_IsAttribute_IntegerRangeType_MultipleValues_AttributeValueContainsWhitespaces(int minValue, int maxValue)
        {
            //arrange
            var attributeMock = new Mock<IAttribute>();
            attributeMock.Setup(e => e.Value).Returns($"{minValue}  - {maxValue}");

            var attributeTypeMock = new Mock<IAttributeType>();
            attributeTypeMock.Setup(e => e.ScalarType).Returns(ScalarType.IntegerRange);

            var nodeTypeMock = new Mock<INodeTypeLinked>();
            nodeTypeMock.Setup(e => e.GetComputedRelationTypes()).Returns(new List<IRelationTypeLinked>());
            nodeTypeMock.Setup(e => e.AttributeType).Returns(attributeTypeMock.Object);

            var nodeMock = new Mock<INode>();
            nodeMock.Setup(e => e.Attribute).Returns(attributeMock.Object);
            nodeMock.Setup(e => e.NodeType).Returns(nodeTypeMock.Object);
            nodeMock.Setup(e => e.OutgoingRelations).Returns(new List<RelationData>());

            //act
            var sut = new ExtNodeService();
            var res = sut.GetExtNode(nodeMock.Object);

            //assert
            var gte = res.AttributeValue.GetType().GetProperty("gte").GetValue(res.AttributeValue, null);
            var lte = res.AttributeValue.GetType().GetProperty("lte").GetValue(res.AttributeValue, null);
            Assert.Equal(minValue.ToString(), gte.ToString());
            Assert.Equal(maxValue.ToString(), lte.ToString());
        }

        [Theory, AutoData]
        public void GetExtNode_IsAttribute_FloatRangeType_SingleValue(int value)
        {
            //arrange
            var attributeMock = new Mock<IAttribute>();
            attributeMock.Setup(e => e.Value).Returns(value.ToString());

            var attributeTypeMock = new Mock<IAttributeType>();
            attributeTypeMock.Setup(e => e.ScalarType).Returns(ScalarType.FloatRange);

            var nodeTypeMock = new Mock<INodeTypeLinked>();
            nodeTypeMock.Setup(e => e.GetComputedRelationTypes()).Returns(new List<IRelationTypeLinked>());
            nodeTypeMock.Setup(e => e.AttributeType).Returns(attributeTypeMock.Object);

            var nodeMock = new Mock<INode>();
            nodeMock.Setup(e => e.Attribute).Returns(attributeMock.Object);
            nodeMock.Setup(e => e.NodeType).Returns(nodeTypeMock.Object);
            nodeMock.Setup(e => e.OutgoingRelations).Returns(new List<RelationData>());

            //act
            var sut = new ExtNodeService();
            var res = sut.GetExtNode(nodeMock.Object);

            //assert
            var gte = res.AttributeValue.GetType().GetProperty("gte").GetValue(res.AttributeValue, null);
            var lte = res.AttributeValue.GetType().GetProperty("lte").GetValue(res.AttributeValue, null);
            Assert.Equal(value.ToString(), gte.ToString());
            Assert.Equal(value.ToString(), lte.ToString());
        }

        [Theory, AutoData]
        public void GetExtNode_IsAttribute_FloatRangeType_MultipleValues(int minValue, int maxValue)
        {
            //arrange
            var attributeMock = new Mock<IAttribute>();
            attributeMock.Setup(e => e.Value).Returns($"{minValue}-{maxValue}");

            var attributeTypeMock = new Mock<IAttributeType>();
            attributeTypeMock.Setup(e => e.ScalarType).Returns(ScalarType.FloatRange);

            var nodeTypeMock = new Mock<INodeTypeLinked>();
            nodeTypeMock.Setup(e => e.GetComputedRelationTypes()).Returns(new List<IRelationTypeLinked>());
            nodeTypeMock.Setup(e => e.AttributeType).Returns(attributeTypeMock.Object);

            var nodeMock = new Mock<INode>();
            nodeMock.Setup(e => e.Attribute).Returns(attributeMock.Object);
            nodeMock.Setup(e => e.NodeType).Returns(nodeTypeMock.Object);
            nodeMock.Setup(e => e.OutgoingRelations).Returns(new List<RelationData>());

            //act
            var sut = new ExtNodeService();
            var res = sut.GetExtNode(nodeMock.Object);

            //assert
            var gte = res.AttributeValue.GetType().GetProperty("gte").GetValue(res.AttributeValue, null);
            var lte = res.AttributeValue.GetType().GetProperty("lte").GetValue(res.AttributeValue, null);
            Assert.Equal(minValue.ToString(), gte.ToString());
            Assert.Equal(maxValue.ToString(), lte.ToString());
        }

        [Theory, AutoData]
        public void GetExtNode_IsAttribute_FloatRangeType_MultipleValues_AttributeValueContainsWhitespaces(int minValue, int maxValue)
        {
            //arrange
            var attributeMock = new Mock<IAttribute>();
            attributeMock.Setup(e => e.Value).Returns($"{minValue}  - {maxValue}");

            var attributeTypeMock = new Mock<IAttributeType>();
            attributeTypeMock.Setup(e => e.ScalarType).Returns(ScalarType.FloatRange);

            var nodeTypeMock = new Mock<INodeTypeLinked>();
            nodeTypeMock.Setup(e => e.GetComputedRelationTypes()).Returns(new List<IRelationTypeLinked>());
            nodeTypeMock.Setup(e => e.AttributeType).Returns(attributeTypeMock.Object);

            var nodeMock = new Mock<INode>();
            nodeMock.Setup(e => e.Attribute).Returns(attributeMock.Object);
            nodeMock.Setup(e => e.NodeType).Returns(nodeTypeMock.Object);
            nodeMock.Setup(e => e.OutgoingRelations).Returns(new List<RelationData>());

            //act
            var sut = new ExtNodeService();
            var res = sut.GetExtNode(nodeMock.Object);

            //assert
            var gte = res.AttributeValue.GetType().GetProperty("gte").GetValue(res.AttributeValue, null);
            var lte = res.AttributeValue.GetType().GetProperty("lte").GetValue(res.AttributeValue, null);
            Assert.Equal(minValue.ToString(), gte.ToString());
            Assert.Equal(maxValue.ToString(), lte.ToString());
        }
    }
}
