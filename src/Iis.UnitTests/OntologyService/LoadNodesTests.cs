using System;
using System.Threading;
using System.Threading.Tasks;

using Iis.DataModel;
using Iis.DbLayer.Ontology.EntityFramework;
using Iis.DbLayer.Repositories;
using Iis.Domain;
using Iis.Interfaces.Elastic;
using Iis.Services.Contracts.Interfaces;

using IIS.Repository.Factories;

using Moq;

using Xunit;

namespace Iis.UnitTests.OntologyService
{
    public class LoadNodesTests
    {
        [Theory, RecursiveAutoData]
        public async Task NodeHasArchivedNodeType_ReturnsNull(Guid nodeId) 
        {
            //arrange
            var ontologyRepositoryMock = new Mock<IOntologyRepository>(MockBehavior.Strict);
            ontologyRepositoryMock.Setup(e => e.GetActiveNodeEntityById(nodeId)).Returns((NodeEntity)null);
            var uowMock = new Mock<IIISUnitOfWork>();
            uowMock.Setup(e => e.OntologyRepository).Returns(ontologyRepositoryMock.Object);
            var uowFacotryMock = new Mock<IUnitOfWorkFactory<IIISUnitOfWork>>(MockBehavior.Strict);

            uowFacotryMock.Setup(e => e.Create()).Returns(uowMock.Object);


            var sut = new OntologyService<IIISUnitOfWork>(
                new Mock<IOntologyModel>().Object,
                new Mock<IElasticService>().Object,
                uowFacotryMock.Object,
                new Mock<IElasticState>().Object);

            //act
            var res = await sut.LoadNodesAsync(nodeId, null, CancellationToken.None);

            //assert
            Assert.Null(res);
        }
    }
}
