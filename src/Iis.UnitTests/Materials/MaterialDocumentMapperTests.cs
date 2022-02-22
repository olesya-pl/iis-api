using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Iis.DataModel.Materials;
using Iis.Domain;
using Iis.Domain.Materials;
using Iis.Domain.Users;
using Iis.Elastic.Entities;
using Iis.Interfaces.Ontology.Data;
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
        private readonly Mock<IOntologyService> _ontologyServiceMock = new Mock<IOntologyService>();
        public MaterialDocumentMapper CreateFixture(Mock<IMapper> mapper, IReadOnlyCollection<Guid> nodeIds)
        {
            _securityLevelChecker.Setup(p => p.AccessGranted(It.IsAny<IReadOnlyList<int>>(), It.IsAny<IReadOnlyList<int>>())).Returns(true);
            _ontologyServiceMock.Setup(e => e.GetNodeIdListByFeatureIdList(It.IsAny<Guid[]>())).Returns(new Guid[] { });

            foreach (var nodeId in nodeIds)
            {
                var originalNodeTypeMock = new Mock<INodeTypeLinked>();
                originalNodeTypeMock.Setup(e => e.IsObjectOfStudy).Returns(true);
                originalNodeTypeMock.Setup(e => e.IsObjectSign).Returns(false);
                var originalNodeMock = new Mock<INode>();
                originalNodeMock.Setup(e => e.GetSecurityLevelIndexes()).Returns(new[] { 1 });
                originalNodeMock.Setup(e => e.NodeType).Returns(originalNodeTypeMock.Object);
                var node = new Entity(nodeId, new Mock<INodeTypeLinked>().Object)
                {
                    OriginalNode = originalNodeMock.Object
                };
                _ontologyServiceMock.Setup(e => e.GetNode(nodeId)).Returns(node);
            }
            return new MaterialDocumentMapper(
                    mapper.Object,
                    new Mock<IOntologySchema>().Object,
                    _ontologyServiceMock.Object,
                    new NodeToJObjectMapper(),
                    new ForbiddenEntityReplacer(_securityLevelChecker.Object, _ontologyServiceMock.Object),
                    new Mock<ISecurityLevelChecker>().Object);
        }

        [Theory]
        [RecursiveAutoData]
        public void CanBeEdited_EditorIsNull_NotInProcessing_ReturnsTrue(MaterialDocument document, Material material, User user, Guid[] nodeIds)
        {
            //arrange
            document.Editor = null;
            document.ProcessedStatus = new Elastic.Entities.MaterialSign { Id = MaterialEntity.ProcessingStatusNotProcessedSignId };
            document.NodeIds = nodeIds;

            var mapperMock = new Mock<IMapper>();
            mapperMock.Setup(e => e.Map<Material>(document)).Returns(material);
            var ids = new List<Guid>();
            ids.AddRange(material.RelatedEventCollection.Select(p => p.Id));
            ids.AddRange(material.RelatedObjectCollection.Select(p => p.Id));
            ids.AddRange(material.RelatedSignCollection.Select(p => p.Id));
            ids.AddRange(nodeIds);
            var sut = CreateFixture(mapperMock, ids);

            //act
            var res = sut.Map(document, user);

            //assert
            Assert.True(res.CanBeEdited);
        }

        [Theory]
        [RecursiveAutoData]
        public void CanBeEdited_EditorIsOtherUser_NotInProcessing_ReturnsTrue(MaterialDocument document, Material material, User user, Guid otherUserId, Guid[] nodeIds)
        {
            //arrange
            document.Editor = new Editor { Id = otherUserId };
            document.ProcessedStatus = new Elastic.Entities.MaterialSign { Id = MaterialEntity.ProcessingStatusNotProcessedSignId };

            var mapperMock = new Mock<IMapper>();
            mapperMock.Setup(e => e.Map<Material>(document)).Returns(material);
            var ids = new List<Guid>();
            ids.AddRange(material.RelatedEventCollection.Select(p => p.Id));
            ids.AddRange(material.RelatedObjectCollection.Select(p => p.Id));
            ids.AddRange(material.RelatedSignCollection.Select(p => p.Id));
            ids.AddRange(nodeIds);
            var sut = CreateFixture(mapperMock, ids);

            //act
            var res = sut.Map(document, user);

            //assert
            Assert.True(res.CanBeEdited);
        }

        [Theory]
        [RecursiveAutoData]
        public void CanBeEdited_EditorIsSameUser_NotInProcessing_ReturnsTrue(MaterialDocument document, Material material, User user, Guid[] nodeIds)
        {
            //arrange
            document.Editor = new Editor { Id = user.Id };
            document.ProcessedStatus = new Elastic.Entities.MaterialSign { Id = MaterialEntity.ProcessingStatusNotProcessedSignId };

            var mapperMock = new Mock<IMapper>();
            mapperMock.Setup(e => e.Map<Material>(document)).Returns(material);
            var ids = new List<Guid>();
            ids.AddRange(material.RelatedEventCollection.Select(p => p.Id));
            ids.AddRange(material.RelatedObjectCollection.Select(p => p.Id));
            ids.AddRange(material.RelatedSignCollection.Select(p => p.Id));
            ids.AddRange(nodeIds);
            var sut = CreateFixture(mapperMock, ids);

            //act
            var res = sut.Map(document, user);

            //assert
            Assert.True(res.CanBeEdited);
        }

        [Theory]
        [RecursiveAutoData]
        public void CanBeEdited_EditorIsNull_InProcessing_ReturnsTrue(MaterialDocument document, Material material, User user, Guid[] nodeIds)
        {
            //arrange
            document.Editor = null;
            document.ProcessedStatus = new Elastic.Entities.MaterialSign { Id = MaterialEntity.ProcessingStatusProcessingSignId };

            var mapperMock = new Mock<IMapper>();
            mapperMock.Setup(e => e.Map<Material>(document)).Returns(material);
            var ids = new List<Guid>();
            ids.AddRange(material.RelatedEventCollection.Select(p => p.Id));
            ids.AddRange(material.RelatedObjectCollection.Select(p => p.Id));
            ids.AddRange(material.RelatedSignCollection.Select(p => p.Id));
            ids.AddRange(nodeIds);
            var sut = CreateFixture(mapperMock, ids);

            //act
            var res = sut.Map(document, user);

            //assert
            Assert.True(res.CanBeEdited);
        }

        [Theory]
        [RecursiveAutoData]
        public void CanBeEdited_EditorIsOtherUser_InProcessing_ReturnsTrue(MaterialDocument document, Material material, User user, Guid otherUserId, Guid[] nodeIds)
        {
            //arrange
            document.Editor = new Editor { Id = otherUserId };
            document.ProcessedStatus = new Elastic.Entities.MaterialSign { Id = MaterialEntity.ProcessingStatusProcessingSignId };

            var mapperMock = new Mock<IMapper>();
            mapperMock.Setup(e => e.Map<Material>(document)).Returns(material);
            var ids = new List<Guid>();
            ids.AddRange(material.RelatedEventCollection.Select(p => p.Id));
            ids.AddRange(material.RelatedObjectCollection.Select(p => p.Id));
            ids.AddRange(material.RelatedSignCollection.Select(p => p.Id));
            ids.AddRange(nodeIds);
            var sut = CreateFixture(mapperMock, ids);

            //act
            var res = sut.Map(document, user);

            //assert
            Assert.False(res.CanBeEdited);
        }

        [Theory]
        [RecursiveAutoData]
        public void CanBeEdited_EditorIsSameUser_InProcessing_ReturnsTrue(MaterialDocument document, Material material, User user, Guid[] nodeIds)
        {
            //arrange
            document.Editor = new Editor { Id = user.Id };
            document.ProcessedStatus = new Elastic.Entities.MaterialSign { Id = MaterialEntity.ProcessingStatusProcessingSignId };

            var mapperMock = new Mock<IMapper>();
            mapperMock.Setup(e => e.Map<Material>(document)).Returns(material);
            var ids = new List<Guid>();
            ids.AddRange(material.RelatedEventCollection.Select(p => p.Id));
            ids.AddRange(material.RelatedObjectCollection.Select(p => p.Id));
            ids.AddRange(material.RelatedSignCollection.Select(p => p.Id));
            ids.AddRange(nodeIds);
            var sut = CreateFixture(mapperMock, ids);

            //act
            var res = sut.Map(document, user);

            //assert
            Assert.True(res.CanBeEdited);
        }
    }
}
