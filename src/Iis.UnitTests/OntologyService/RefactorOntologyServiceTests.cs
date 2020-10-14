﻿using DeepEqual.Syntax;
using Iis.DataModel;
using Iis.DbLayer.Ontology.EntityFramework;
using Iis.DbLayer.OntologySchema;
using Iis.DbLayer.Repositories;
using Iis.Domain;
using Iis.Interfaces.Elastic;
using Iis.Interfaces.Ontology.Schema;
using Iis.OntologyData;
using Iis.OntologyModelWrapper;
using Iis.OntologySchema;
using Iis.Services.Contracts.Interfaces;
using IIS.Repository.Factories;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Iis.UnitTests.OntologyService
{
    public class RefactorOntologyServiceTests
    {
        OntologyNodesData _data;
        IOntologySchema _schema;
        IOntologyService _oldService;
        IOntologyService _newService;
        OntologyContext _context;

        [Fact]
        public async Task Test()
        {
            Initialize("Server=localhost;Database=contour_dev_net;Username=postgres;Password=123");
            //await GetIncomingEntitiesTest();
            await GetNodesAsyncTest();
        }
        private async Task GetIncomingEntitiesTest()
        {
            var entityTypes = _schema.GetAllNodeTypes().Where(nt => nt.Kind == Kind.Entity);
            foreach (var entityType in entityTypes)
            {
                var nodes = _data.GetNodesByTypeId(entityType.Id);
                foreach (var node in nodes)
                {
                    var oldIncomingRelations = await _oldService.GetIncomingEntities(node.Id);
                    var newIncomingRelations = await _newService.GetIncomingEntities(node.Id);
                    AssertIncomingRelationLists(
                        oldIncomingRelations.OrderBy(r => r.RelationId).ToList(), 
                        newIncomingRelations.OrderBy(r => r.RelationId).ToList());
                }
            }
        }
        private async Task GetNodesAsyncTest()
        {
            var nodeIds = _data.Nodes.OrderBy(n => n.Id).Select(n => n.Id).Take(50);
            var oldResult = await _oldService.GetNodesAsync(nodeIds);
            var newResult = await _newService.GetNodesAsync(nodeIds);
            
            Assert.Equal(oldResult.count, newResult.count);
            AssertNodeLists(oldResult.nodes, newResult.nodes);
        }
        private void Initialize(string connectionString)
        {
            _context = OntologyContext.GetContext(connectionString);

            var source = new OntologySchemaSource { Data = connectionString, SourceKind = SchemaSourceKind.Database };
            var ontologySchemaService = new OntologySchemaService();
            _schema = ontologySchemaService.LoadFromDatabase(source);

            var ontologyModel = new OntologyWrapper(_schema);
            var dbContextOptions = new DbContextOptionsBuilder().UseNpgsql(connectionString).Options;
            
            var ontologyRepository = new OntologyRepository();
            ontologyRepository.SetContext(_context);
            var uowMock = new Mock<IIISUnitOfWork>();
            uowMock.Setup(e => e.OntologyRepository).Returns(ontologyRepository);

            var uowFacotryMock = new Mock<IUnitOfWorkFactory<IIISUnitOfWork>>(MockBehavior.Strict);
            uowFacotryMock.Setup(e => e.Create()).Returns(uowMock.Object);

            var elasticService = new Mock<IElasticService>();
            var elasticState = new Mock<IElasticState>();
            _oldService = new OntologyService<IIISUnitOfWork>(
                ontologyModel, elasticService.Object, uowFacotryMock.Object, elasticState.Object);

            var rawData = new NodesRawData(_context.Nodes, _context.Relations, _context.Attributes);
            _data = new OntologyNodesData(rawData, _schema);
            _newService = new OntologyServiceWithCache(_data, elasticService.Object);
        }
        private void AssertIncomingRelations(IncomingRelation ir1, IncomingRelation ir2)
        {
            Assert.Equal(ir1.RelationId, ir2.RelationId);
            Assert.Equal(ir1.RelationTypeName, ir2.RelationTypeName);
            Assert.Equal(ir1.RelationTypeTitle, ir2.RelationTypeTitle);
            Assert.Equal(ir1.EntityId, ir2.EntityId);
            Assert.Equal(ir1.EntityTypeName, ir2.EntityTypeName);
            Assert.Equal(ir1.Entity?.Id, ir2.Entity?.Id);
        }
        private void AssertIncomingRelationLists(IReadOnlyList<IncomingRelation> list1, IReadOnlyList<IncomingRelation> list2)
        {
            Assert.Equal(list1.Count, list2.Count);
            for (int i = 0; i < list1.Count; i++)
            {
                AssertIncomingRelations(list1[i], list2[i]);
            }
        }
        private void AssertNodes(Node oldNode, Node newNode)
        {
            if (oldNode == null && newNode == null) return;

            Assert.Equal(oldNode.GetType(), newNode.GetType());
            Assert.Equal(oldNode.Id, newNode.Id);
            Assert.Equal(oldNode.Type?.Id, newNode.Type?.Id);
            Assert.Equal(oldNode.CreatedAt, newNode.CreatedAt);
            Assert.Equal(oldNode.UpdatedAt, newNode.UpdatedAt);
            AssertNodeLists(oldNode.Nodes, newNode.Nodes);

            if (oldNode is Domain.Attribute)
            {
                AssertAttributes(oldNode as Domain.Attribute, newNode as Domain.Attribute);
            }

            if (oldNode is Relation)
            {
                AssertRelations(oldNode as Relation, newNode as Relation);
            }
        }
        private void AssertAttributes(Domain.Attribute oldAttribute, Domain.Attribute newAttribute)
        {
            Assert.Equal(oldAttribute.Value, newAttribute.Value);
        }
        private void AssertRelations(Relation oldRelation, Relation newRelation)
        {
            AssertNodes(oldRelation.Target, newRelation.Target);
            AssertNodes(oldRelation.AttributeTarget, newRelation.AttributeTarget);
            AssertNodes(oldRelation.EntityTarget, newRelation.EntityTarget);
        }
        private void AssertNodeLists(IEnumerable<Node> oldNodes, IEnumerable<Node> newNodes)
        {
            var oldOrdered = oldNodes.OrderBy(n => n.Id).ToList();
            var newOrdered = newNodes.OrderBy(n => n.Id).ToList();

            Assert.Equal(oldOrdered.Count, newOrdered.Count);
            for (int i = 0; i < oldOrdered.Count; i++)
            {
                AssertNodes(oldOrdered[i], newOrdered[i]);
            }
        }
    }
}
