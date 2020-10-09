using DeepEqual.Syntax;
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
            Initialize("Server=localhost;Database=contour_dev_net;Username=postgres;Password = 123");
            var oldNodes = await _oldService.GetIncomingEntities(new Guid("05dc8ec8-0969-4dc6-a0c3-5703cdcf616b"));
            var newNodes = await _newService.GetIncomingEntities(new Guid("05dc8ec8-0969-4dc6-a0c3-5703cdcf616b"));
            oldNodes.ShouldDeepEqual(newNodes);
            //Assert.Equal(0, nodes.Count);
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
    }
}
