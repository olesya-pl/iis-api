using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Iis.DbLayer.Ontology.EntityFramework;
using Iis.Interfaces.Elastic;
using Iis.Interfaces.Ontology.Data;
using Iis.Interfaces.Ontology.Schema;
using Iis.OntologyData.DataTypes;
using Iis.Services.Contracts.Interfaces;
using Moq;
using Xunit;

namespace Iis.UnitTests.Iis.DbLayer
{
    public class OntologyServiceWithCacheTests
    {
        private static Mock<IOntologyNodesData> SetupNodesData(
            Guid nodeTypeId, 
            string attributeValue, 
            Guid nodeId, 
            List<RelationData> incomingRelations, 
            List<RelationData> outgoingRelations)
        {
            var valueTypeMock = new Mock<INodeTypeLinked>();
            valueTypeMock.Setup(e => e.Name).Returns("value");

            var sourceNodeMock = new Mock<INode>();
            sourceNodeMock.Setup(e => e.NodeTypeId).Returns(nodeTypeId);
            sourceNodeMock.Setup(e => e.NodeType).Returns(new Mock<INodeTypeLinked>().Object);

            var targetNodeMock = new Mock<INode>();
            targetNodeMock.Setup(e => e.NodeType).Returns(new Mock<INodeTypeLinked>().Object);

            var incomingRelationsMock = new Mock<IRelation>();
            incomingRelationsMock.Setup(e => e.SourceNode).Returns(sourceNodeMock.Object);
            incomingRelationsMock.Setup(e => e.TargetNode).Returns(targetNodeMock.Object);

            var allIncomingRelations = new List<IRelation>();
            allIncomingRelations.Add(incomingRelationsMock.Object);
            allIncomingRelations.AddRange(incomingRelations);

            var attributeData = new Mock<IAttribute>();
            attributeData.Setup(e => e.Id).Returns(nodeId);
            attributeData.Setup(e => e.Value).Returns(attributeValue);

            var resultNodeMock = new Mock<INode>();
            resultNodeMock.Setup(e => e.Id).Returns(nodeId);
            resultNodeMock.Setup(e => e.NodeType).Returns(valueTypeMock.Object);
            resultNodeMock.Setup(e => e.Value).Returns(attributeValue);
            resultNodeMock.Setup(e => e.IncomingRelations).Returns(allIncomingRelations);
            resultNodeMock.Setup(e => e.OutgoingRelations).Returns(outgoingRelations);
            resultNodeMock.Setup(e => e.Attribute).Returns(attributeData.Object);

            var allNodes = new List<INode>();
            allNodes.Add(resultNodeMock.Object);

            var nodesDataMock = new Mock<IOntologyNodesData>();
            nodesDataMock.Setup(e => e.Nodes).Returns(allNodes);
            return nodesDataMock;
        }

        [Theory, RecursiveAutoData]
        public void GetNodesByUniqueValue_MatchesExactValue(Guid nodeTypeId,
            string attributeValue,
            int limit,
            Guid nodeId,
            List<RelationData> incomingRelations,
            List<RelationData> outgoingRelations)
        {
            Mock<IOntologyNodesData> nodesDataMock =
                SetupNodesData(nodeTypeId, attributeValue, nodeId, incomingRelations, outgoingRelations);

            var sut = new OntologyServiceWithCache(nodesDataMock.Object, 
                new Mock<IElasticService>().Object,
                new Mock<IElasticState>().Object);

            var res = sut.GetNodesByUniqueValue(nodeTypeId, attributeValue, "value", limit);
            Assert.Single(res);
            Assert.Equal(nodeId, res.First().Id);
            Assert.Equal(attributeValue, res.First().Value);
        }

        [Theory, RecursiveAutoData]
        public void GetNodesByUniqueValue_MatchesStartValue(Guid nodeTypeId,
            string attributeValue,
            int limit,
            Guid nodeId,
            List<RelationData> incomingRelations,
            List<RelationData> outgoingRelations)
        {
            Mock<IOntologyNodesData> nodesDataMock =
                SetupNodesData(nodeTypeId, attributeValue + "hitler", nodeId, incomingRelations, outgoingRelations);

            var sut = new OntologyServiceWithCache(nodesDataMock.Object,
                new Mock<IElasticService>().Object,
                new Mock<IElasticState>().Object);

            var res = sut.GetNodesByUniqueValue(nodeTypeId, attributeValue, "value", limit);
            Assert.Single(res);
            Assert.Equal(nodeId, res.First().Id);
            Assert.Equal(attributeValue + "hitler", res.First().Value);
        }

        [Theory, RecursiveAutoData]
        public void GetNodesByUniqueValue_MatchesEndValue(Guid nodeTypeId,
            string attributeValue,
            int limit,
            Guid nodeId,
            List<RelationData> incomingRelations,
            List<RelationData> outgoingRelations)
        {
            Mock<IOntologyNodesData> nodesDataMock =
                SetupNodesData(nodeTypeId, "bandera" + attributeValue, nodeId, incomingRelations, outgoingRelations);

            var sut = new OntologyServiceWithCache(nodesDataMock.Object, 
                new Mock<IElasticService>().Object,
                new Mock<IElasticState>().Object);

            var res = sut.GetNodesByUniqueValue(nodeTypeId, attributeValue, "value", limit);
            Assert.Single(res);
            Assert.Equal(nodeId, res.First().Id);
            Assert.Equal("bandera" + attributeValue, res.First().Value);
        }

        [Theory, RecursiveAutoData]
        public void GetNodesByUniqueValue_NodeValueNull_DoesNotThrow(Guid nodeTypeId,
            string attributeValue,
            int limit,
            Guid nodeId,
            List<RelationData> incomingRelations,
            List<RelationData> outgoingRelations)
        {
            Mock<IOntologyNodesData> nodesDataMock =
                SetupNodesData(nodeTypeId, null, nodeId, incomingRelations, outgoingRelations);

            var sut = new OntologyServiceWithCache(nodesDataMock.Object, 
                new Mock<IElasticService>().Object,
                new Mock<IElasticState>().Object);

            var res = sut.GetNodesByUniqueValue(nodeTypeId, attributeValue, "value", limit);
            Assert.Empty(res);
        }
    }
}
