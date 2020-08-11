using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Iis.DataModel;
using Iis.DbLayer.Ontology.EntityFramework;
using Iis.Domain;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Iis.UnitTests.OntologyRepositoryTests
{
    public class GetNodesWithSuggestionTests
    {
        [Theory, RecursiveAutoData]
        public async Task GetNodesWithoutSuggestion_NoSuggestion_SelectAll(List<NodeEntity> nodes,
            NodeTypeEntity expectedType,
            NodeTypeEntity otherType,
            List<NodeEntity> nodesForRelations)
        {
            //arrange
            var serviceProvider = Utils.GetServiceProvider();
            var context = serviceProvider.GetRequiredService<OntologyContext>();
            expectedType.Nodes = new List<NodeEntity>();
            nodes[0].NodeType = expectedType;
            nodes[0].NodeTypeId = expectedType.Id;
            nodes[0].IsArchived = false;
            for (var i = 0; i < nodes[0].OutgoingRelations.Count(); i++)
            {
                var relation = nodes[0].OutgoingRelations.Skip(i).First();
                var nodeForRelations = nodesForRelations[i % nodesForRelations.Count()];
                nodeForRelations.IsArchived = false;
                relation.Node = nodeForRelations;
                relation.Node.Id = nodeForRelations.Id;
            }
            foreach (var node in nodes.Skip(1))
            {
                node.NodeType = otherType;
                node.NodeTypeId = otherType.Id;
            }
            context.Nodes.AddRange(nodes);
            context.Nodes.AddRange(nodesForRelations);
            context.Relations.AddRange(nodes.SelectMany(e => e.OutgoingRelations));
            context.Relations.AddRange(nodes.SelectMany(e => e.IncomingRelations));
            context.SaveChanges();

            var repository = serviceProvider.GetRequiredService<IOntologyRepository>();
            var sut = repository as OntologyRepository;
            sut.SetContext(context);

            //act
            var res = await sut.GetNodesWithSuggestionAsync(new[] { expectedType.Id }, new ElasticFilter {
                Limit = 50,
                Offset = 0
            });

            //assert
            Assert.Single(res);
            Assert.Equal(nodes[0].Id, res[0].Id);
        }

        [Theory, RecursiveAutoData]
        public async Task GetNodesWithoutSuggestion_NoSuggestion_NodeIsArchived(List<NodeEntity> nodes,
            NodeTypeEntity expectedType,
            NodeTypeEntity otherType,
            List<NodeEntity> nodesForRelations)
        {
            //arrange
            var serviceProvider = Utils.GetServiceProvider();
            var context = serviceProvider.GetRequiredService<OntologyContext>();
            expectedType.Nodes = new List<NodeEntity>();
            nodes[0].NodeType = expectedType;
            nodes[0].NodeTypeId = expectedType.Id;
            nodes[0].IsArchived = true;
            foreach (var node in nodes.Skip(1))
            {
                node.NodeType = otherType;
                node.NodeTypeId = otherType.Id;

            }
            for (var i = 0; i < nodes[0].OutgoingRelations.Count(); i++)
            {
                var relation = nodes[0].OutgoingRelations.Skip(i).First();
                var nodeForRelations = nodesForRelations[i % nodesForRelations.Count()];
                nodeForRelations.IsArchived = false;
                relation.Node = nodeForRelations;
                relation.Node.Id = nodeForRelations.Id;
            }
            context.Nodes.AddRange(nodes);
            context.Nodes.AddRange(nodesForRelations);
            context.Relations.AddRange(nodes.SelectMany(e => e.OutgoingRelations));
            context.Relations.AddRange(nodes.SelectMany(e => e.IncomingRelations));
            context.SaveChanges();

            var repository = serviceProvider.GetRequiredService<IOntologyRepository>();
            var sut = repository as OntologyRepository;
            sut.SetContext(context);

            //act
            var res = await sut.GetNodesWithSuggestionAsync(new[] { expectedType.Id }, new ElasticFilter
            {
                Limit = 50,
                Offset = 0
            });

            //assert
            Assert.Empty(res);
        }

        [Theory, RecursiveAutoData]
        public async Task GetNodesWithoutSuggestion_Suggestion_NodeExists(List<NodeEntity> nodes,
            NodeTypeEntity expectedType,
            NodeTypeEntity otherType,
            List<NodeEntity> nodesForRelations,
            NodeEntity targetNode)
        {
            //arrange
            var serviceProvider = Utils.GetServiceProvider();
            var context = serviceProvider.GetRequiredService<OntologyContext>();
            expectedType.Nodes = new List<NodeEntity>();
            nodes[0].NodeType = expectedType;
            nodes[0].NodeTypeId = expectedType.Id;
            nodes[0].IsArchived = false;
            for (var i = 0; i < nodes[0].OutgoingRelations.Count(); i++)
            {
                var relation = nodes[0].OutgoingRelations.Skip(i).First();
                var nodeForRelations = nodesForRelations[i % nodesForRelations.Count()];
                nodeForRelations.IsArchived = false;
                relation.Node = nodeForRelations;
                relation.Node.Id = nodeForRelations.Id;
            }
            nodes[0].OutgoingRelations.First().TargetNode = targetNode;
            nodes[0].OutgoingRelations.First().TargetNodeId = targetNode.Id;
            foreach (var node in nodes.Skip(1))
            {
                node.NodeType = otherType;
                node.NodeTypeId = otherType.Id;
            }
            context.Nodes.AddRange(nodes);
            context.Nodes.AddRange(nodesForRelations);
            context.Nodes.Add(targetNode);
            context.Relations.AddRange(nodes.SelectMany(e => e.OutgoingRelations));
            context.Relations.AddRange(nodes.SelectMany(e => e.IncomingRelations));
            context.SaveChanges();

            var repository = serviceProvider.GetRequiredService<IOntologyRepository>();
            var sut = repository as OntologyRepository;
            sut.SetContext(context);

            //act
            var res = await sut.GetNodesWithSuggestionAsync(new[] { expectedType.Id }, new ElasticFilter
            {
                Limit = 50,
                Offset = 0,
                Suggestion = targetNode.Attribute.Value
            });

            //assert
            Assert.Single(res);
            Assert.Equal(nodes[0].Id, res[0].Id);
        }
    }
}
