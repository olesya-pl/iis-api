using Iis.Interfaces.Ontology.Schema;
using Iis.OntologyData.DataTypes;
using Moq;
using Xunit;

namespace Iis.UnitTests.Iis.OntologyData
{
    public class NodeDataTests
    {
        private static void SetupRelation(NodeData sourceNode, NodeData node, NodeData targetNode,
            AttributeData attribute, RelationData outgoingRelation, string propertyName, string value)
        {
            attribute.Value = value;
            targetNode.Attribute = attribute;

            var nodeTypeMock = new Mock<INodeTypeLinked>();
            nodeTypeMock.Setup(e => e.Name).Returns(propertyName);
            node.NodeType = nodeTypeMock.Object;

            outgoingRelation._targetNode = targetNode;
            outgoingRelation._sourceNode = sourceNode;
            outgoingRelation._node = node;
            sourceNode._outgoingRelations.Add(outgoingRelation);
        }

        [Theory, RecursiveAutoData]
        public void HasPropertyWithValue_NodeHasProperty_WithGivenValue_ReturnsTrue(NodeData sourceNode,
            NodeData node,
            NodeData targetNode,
            AttributeData attribute,
            RelationData outgoingRelation,
            string propertyName,
            string value)
        {
            //arrange
            SetupRelation(sourceNode, node, targetNode,
                attribute, outgoingRelation, propertyName, value);

            //assert
            Assert.True(sourceNode.HasPropertyWithValue(propertyName, value));
        }

        [Theory, RecursiveAutoData]
        public void HasPropertyWithValue_NodeHasProperty_WithWrongValue_ReturnsFalse(NodeData sourceNode,
            NodeData node,
            NodeData targetNode,
            AttributeData attribute,
            RelationData outgoingRelation,
            string propertyName,
            string value)
        {
            //arrange
            SetupRelation(sourceNode, node, targetNode,
                attribute, outgoingRelation, propertyName, $"not{value}");

            //assert
            Assert.False(sourceNode.HasPropertyWithValue(propertyName, value));
        }

        [Theory, RecursiveAutoData]
        public void HasPropertyWithValue_NodeHasNoProperty_ReturnsFalse(NodeData sourceNode,
            string propertyName,
            string value)
        {
            //assert
            Assert.False(sourceNode.HasPropertyWithValue(propertyName, value));
        }
    }
}
