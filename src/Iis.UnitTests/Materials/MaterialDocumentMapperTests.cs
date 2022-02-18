using System;
using AutoMapper;
using Iis.DataModel.Materials;
using Iis.Domain;
using Iis.Domain.Materials;
using Iis.Domain.Users;
using Iis.Elastic.Entities;
using Iis.Interfaces.Ontology.Schema;
using Iis.Interfaces.SecurityLevels;
using Iis.Services;
using IIS.Services.Materials;
using Moq;
using Xunit;

namespace Iis.UnitTests.Materials
{
    public class MaterialDocumentMapperTests
    {
        private readonly Mock<ISecurityLevelChecker> _securityLevelChecker = new Mock<ISecurityLevelChecker>();
        private readonly Mock<IOntologyService> _ontologyService = new Mock<IOntologyService>();
        public MaterialDocumentMapper CreateFixture(Mock<IMapper> mapper)
        {
            var ontologyServiceMock = new Mock<IOntologyService>();
            ontologyServiceMock.Setup(e => e.GetNodeIdListByFeatureIdList(It.IsAny<Guid[]>())).Returns(new Guid[] { });
            return new MaterialDocumentMapper(
                    mapper.Object,
                    new Mock<IOntologySchema>().Object,
                    ontologyServiceMock.Object,
                    new NodeToJObjectMapper(),
                    new ForbiddenEntityReplacer(_securityLevelChecker.Object, _ontologyService.Object),
                    new Mock<ISecurityLevelChecker>().Object);
        }

        [Theory]
        [RecursiveAutoData]
        public void CanBeEdited_EditorIsNull_NotInProcessing_ReturnsTrue(MaterialDocument document, Material material, User user)
        {
            //arrange
            document.Editor = null;
            document.ProcessedStatus = new Elastic.Entities.MaterialSign { Id = MaterialEntity.ProcessingStatusNotProcessedSignId };

            var mapperMock = new Mock<IMapper>();
            mapperMock.Setup(e => e.Map<Material>(document)).Returns(material);
            var sut = CreateFixture(mapperMock);

            //act
            var res = sut.Map(document, user);

            //assert
            Assert.True(res.CanBeEdited);
        }

        [Theory]
        [RecursiveAutoData]
        public void CanBeEdited_EditorIsOtherUser_NotInProcessing_ReturnsTrue(MaterialDocument document, Material material, User user, Guid otherUserId)
        {
            //arrange
            document.Editor = new Editor { Id = otherUserId };
            document.ProcessedStatus = new Elastic.Entities.MaterialSign { Id = MaterialEntity.ProcessingStatusNotProcessedSignId };

            var mapperMock = new Mock<IMapper>();
            mapperMock.Setup(e => e.Map<Material>(document)).Returns(material);
            var sut = CreateFixture(mapperMock);

            //act
            var res = sut.Map(document, user);

            //assert
            Assert.True(res.CanBeEdited);
        }

        [Theory]
        [RecursiveAutoData]
        public void CanBeEdited_EditorIsSameUser_NotInProcessing_ReturnsTrue(MaterialDocument document, Material material, User user)
        {
            //arrange
            document.Editor = new Editor { Id = user.Id };
            document.ProcessedStatus = new Elastic.Entities.MaterialSign { Id = MaterialEntity.ProcessingStatusNotProcessedSignId };

            var mapperMock = new Mock<IMapper>();
            mapperMock.Setup(e => e.Map<Material>(document)).Returns(material);
            var sut = CreateFixture(mapperMock);

            //act
            var res = sut.Map(document, user);

            //assert
            Assert.True(res.CanBeEdited);
        }

        [Theory]
        [RecursiveAutoData]
        public void CanBeEdited_EditorIsNull_InProcessing_ReturnsTrue(MaterialDocument document, Material material, User user)
        {
            //arrange
            document.Editor = null;
            document.ProcessedStatus = new Elastic.Entities.MaterialSign { Id = MaterialEntity.ProcessingStatusProcessingSignId };

            var mapperMock = new Mock<IMapper>();
            mapperMock.Setup(e => e.Map<Material>(document)).Returns(material);
            var sut = CreateFixture(mapperMock);

            //act
            var res = sut.Map(document, user);

            //assert
            Assert.True(res.CanBeEdited);
        }

        [Theory]
        [RecursiveAutoData]
        public void CanBeEdited_EditorIsOtherUser_InProcessing_ReturnsTrue(MaterialDocument document, Material material, User user, Guid otherUserId)
        {
            //arrange
            document.Editor = new Editor { Id = otherUserId };
            document.ProcessedStatus = new Elastic.Entities.MaterialSign { Id = MaterialEntity.ProcessingStatusProcessingSignId };

            var mapperMock = new Mock<IMapper>();
            mapperMock.Setup(e => e.Map<Material>(document)).Returns(material);
            var sut = CreateFixture(mapperMock);

            //act
            var res = sut.Map(document, user);

            //assert
            Assert.False(res.CanBeEdited);
        }

        [Theory]
        [RecursiveAutoData]
        public void CanBeEdited_EditorIsSameUser_InProcessing_ReturnsTrue(MaterialDocument document, Material material, User user)
        {
            //arrange
            document.Editor = new Editor { Id = user.Id };
            document.ProcessedStatus = new Elastic.Entities.MaterialSign { Id = MaterialEntity.ProcessingStatusProcessingSignId };

            var mapperMock = new Mock<IMapper>();
            mapperMock.Setup(e => e.Map<Material>(document)).Returns(material);
            var sut = CreateFixture(mapperMock);

            //act
            var res = sut.Map(document, user);

            //assert
            Assert.True(res.CanBeEdited);
        }
    }
}
