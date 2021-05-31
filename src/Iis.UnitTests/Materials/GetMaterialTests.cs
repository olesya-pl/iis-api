using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using AutoMapper;
using Iis.DataModel.Materials;
using Iis.DbLayer.MaterialEnum;
using Iis.DbLayer.Repositories;
using Iis.Domain;
using Iis.Domain.Materials;
using Iis.Domain.Users;
using Iis.Interfaces.Ontology.Data;
using Iis.Interfaces.Ontology.Schema;
using Iis.Services;
using Iis.Services.Contracts.Interfaces;
using IIS.Repository.Factories;
using IIS.Services.Materials;
using Moq;
using Xunit;

namespace Iis.UnitTests.Materials
{
    public class GetMaterialTests
    {
        [Theory, RecursiveAutoData]
        public async Task GetMaterial_UserAccess_ExceedsMaterialAccess(User user, MaterialEntity materialEntity, Material material)
        {
            //arrange
            user.AccessLevel = 2;
            materialEntity.AccessLevel = 1;
            materialEntity.MaterialInfos = new List<MaterialInfoEntity>();

            MaterialProvider<IIISUnitOfWork> sut = CreateMaterialProvider(materialEntity, material);

            //act
            var res = await sut.GetMaterialAsync(materialEntity.Id, user);

            //assert
            Assert.NotNull(res);
        }

        [Theory, RecursiveAutoData]
        public async Task GetMaterial_UserAccess_EqualsMaterialAccess(User user, MaterialEntity materialEntity, Material material)
        {
            //arrange
            user.AccessLevel = 2;
            materialEntity.AccessLevel = 2;
            materialEntity.MaterialInfos = new List<MaterialInfoEntity>();

            MaterialProvider<IIISUnitOfWork> sut = CreateMaterialProvider(materialEntity, material);

            //act
            var res = await sut.GetMaterialAsync(materialEntity.Id, user);

            //assert
            Assert.NotNull(res);
        }

        [Theory, RecursiveAutoData]
        public async Task GetMaterial_UserAccess_IsLowerThanMaterialAccess(User user, MaterialEntity materialEntity, Material material)
        {
            //arrange
            user.AccessLevel = 1;
            materialEntity.AccessLevel = 2;
            materialEntity.MaterialInfos = new List<MaterialInfoEntity>();

            MaterialProvider<IIISUnitOfWork> sut = CreateMaterialProvider(materialEntity, material);

            await Assert.ThrowsAsync<ArgumentException>(async () => await sut.GetMaterialAsync(materialEntity.Id, user));
        }

        private static MaterialProvider<IIISUnitOfWork> CreateMaterialProvider(MaterialEntity materialEntity, Material material)
        {
            var ontologyService = new Mock<IOntologyService>(MockBehavior.Strict);
            var ontologySchema = new Mock<IOntologySchema>(MockBehavior.Strict);
            var ontologyData = new Mock<IOntologyNodesData>(MockBehavior.Strict);
            var materialElasticService = new Mock<IMaterialElasticService>(MockBehavior.Strict);
            var mLResponseRepository = new Mock<IMLResponseRepository>(MockBehavior.Strict);
            var materialSignRepository = new Mock<IMaterialSignRepository>(MockBehavior.Strict);

            var mapper = new Mock<IMapper>();
            mapper.Setup(e => e.Map<Material>(materialEntity)).Returns(material);

            var materialRepository = new Mock<IMaterialRepository>(MockBehavior.Strict);
            materialRepository
                .Setup(e => e.GetByIdAsync(materialEntity.Id, MaterialIncludeEnum.WithChildren, MaterialIncludeEnum.WithFeatures))
                .ReturnsAsync(materialEntity);
            var unitOfWork = new Mock<IIISUnitOfWork>();
            unitOfWork.Setup(e => e.MaterialRepository).Returns(materialRepository.Object);
            var unitOfWorkFactory = new Mock<IUnitOfWorkFactory<IIISUnitOfWork>>();
            unitOfWorkFactory.Setup(e => e.Create()).Returns(unitOfWork.Object);

            var httpClientFactory = new Mock<IHttpClientFactory>(MockBehavior.Strict);

            var sut = new MaterialProvider<IIISUnitOfWork>(ontologyService.Object,
                ontologySchema.Object,
                ontologyData.Object,
                materialElasticService.Object,
                mLResponseRepository.Object,
                materialSignRepository.Object,
                mapper.Object,
                unitOfWorkFactory.Object,
                new Mock<IImageVectorizer>().Object,
                new NodeToJObjectMapper());
            return sut;
        }
    }
}
