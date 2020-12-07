using System.Collections.Generic;
using System.Linq;
using AutoFixture.Xunit2;
using Iis.Api;
using Iis.DbLayer.Repositories;
using Iis.Interfaces.Elastic;
using Iis.Interfaces.Ontology.Schema;
using Iis.OntologySchema.DataTypes;
using Iis.Services;
using Iis.Services.Contracts.Interfaces;
using Iis.Services.Contracts.Interfaces.Elastic;
using IIS.Core.Ontology.EntityFramework;
using Moq;
using Xunit;

namespace Iis.UnitTests.Iis.Elastic.Tests
{
    public class TypesAreSupportedTests
    {
        [Theory, AutoData]
        public void TypesAreSupported_TypeExists(List<SchemaNodeType> ontologytypes)
        {
            //arrange
            var sut = SetupOntologyTypes(ontologytypes);

            //assert
            Assert.True(sut.TypesAreSupported(new[] { ontologytypes.First(p => !p.IsAbstract).Name }));
        }

        [Theory, AutoData]
        public void TypesAreSupported_TypeExists_IsAbstract(List<SchemaNodeType> ontologytypes)
        {
            //arrange
            ontologytypes.First().IsAbstract = true;
            var sut = SetupOntologyTypes(ontologytypes);

            //assert
            Assert.False(sut.TypesAreSupported(new[] { ontologytypes.First(p => p.IsAbstract).Name }));
        }

        [Theory, AutoData]
        public void TypesAreSupported_TypeNotPresent(List<SchemaNodeType> ontologytypes)
        {
            //arrange
            var sut = SetupOntologyTypes(ontologytypes);

            //assert
            Assert.False(sut.TypesAreSupported(new[] { "Hitler" }));
        }

        private static ElasticService SetupOntologyTypes(List<SchemaNodeType> ontologytypes)
        {
            var elasticServiceMock = new Mock<IElasticManager>();
            var elasticConfigurationMock = new Mock<IElasticConfiguration>();
            var ontologySchemaMock = new Mock<IOntologySchema>();
            var nodeRepositoryMock = new Mock<INodeRepository>();
            var materialRepositoryMock = new Mock<IMaterialRepository>();
            var elasticStateMock = new Mock<IElasticState>();
            var elasticResponseManagerFactory = new Mock<IElasticResponseManagerFactory>();

            var objectOfStudyTypeMock = new Mock<INodeTypeLinked>();
            ontologySchemaMock
                .Setup(p => p.GetEntityTypeByName(EntityTypeNames.ObjectOfStudy.ToString()))
                .Returns(objectOfStudyTypeMock.Object);

            objectOfStudyTypeMock.Setup(p => p.GetAllDescendants())
                .Returns(ontologytypes);

            //act
            var sut = new ElasticService(
                elasticServiceMock.Object,
                elasticConfigurationMock.Object,
                nodeRepositoryMock.Object,
                new ElasticState(ontologySchemaMock.Object));
            return sut;
        }
    }
}
