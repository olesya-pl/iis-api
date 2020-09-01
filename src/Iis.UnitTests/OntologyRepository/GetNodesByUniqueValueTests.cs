using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Iis.DataModel;
using Iis.DbLayer.Ontology.EntityFramework;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Iis.UnitTests.OntologyRepositoryTests
{
    public class GetNodesByUniqueValueTests
    {
        [Theory, RecursiveAutoData]
        public async Task GetNodesByUniqueValue_Distinct(
            NodeEntity sourceNode,
            NodeEntity node,
            AttributeEntity attribute,
            NodeTypeEntity nodeType,
            NodeTypeEntity sourceNodeType,
            List<RelationEntity> relations,
            string value,
            string valueTypeName)
        {
            //arrange
            var serviceProvider = Utils.GetServiceProvider();
            var context = serviceProvider.GetRequiredService<OntologyContext>();

            attribute.Id = node.Id;
            attribute.Node = node;
            attribute.Value = value;

            node.Attribute = attribute;
            node.NodeType = nodeType;
            node.NodeTypeId = nodeType.Id;
            node.IncomingRelations = relations;
            node.IsArchived = false;

            nodeType.Name = valueTypeName;

            sourceNode.NodeTypeId = sourceNodeType.Id;
            sourceNode.NodeType = sourceNodeType;
            sourceNode.OutgoingRelations = relations;
            sourceNode.IsArchived = false;

            foreach (var relation in relations)
            {
                relation.TargetNode = node;
                relation.TargetNodeId = node.Id;
                relation.SourceNode = sourceNode;
                relation.SourceNodeId = sourceNode.Id;
            }

            context.Add(node);
            context.Add(sourceNode);
            context.Add(attribute);
            context.Add(nodeType);
            context.Add(sourceNodeType);
            context.AddRange(relations);

            context.SaveChanges();

            var repository = serviceProvider.GetRequiredService<IOntologyRepository>();
            var sut = repository as OntologyRepository;
            sut.SetContext(context);

            //act
            var res = await sut.GetNodesByUniqueValue(sourceNodeType.Id, value, valueTypeName);

            //assert
            Assert.Single(res);
        }
    }
}
