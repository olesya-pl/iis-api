using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using AutoMapper;

using Xunit;
using Iis.DataModel;
using Iis.DataModel.Materials;
using Microsoft.Extensions.Configuration;
using Iis.DbLayer.Repositories;
using Moq;
using AutoFixture.Xunit2;
using IIS.Core.Materials.EntityFramework;
using Iis.Domain;
using Iis.Interfaces.Elastic;
using Iis.Interfaces.Ontology.Data;
using Iis.Interfaces.Ontology.Schema;
using Iis.Services.Contracts.Interfaces;
using System.Net.Http;
using Microsoft.AspNetCore.Http;
using Iis.Utility;

namespace Iis.UnitTests.Materials
{
    public class MaterialUpdateTests : IDisposable
    {
        private readonly MaterialProvider<IIISUnitOfWork> materialProvider;
        private readonly IServiceProvider _serviceProvider;
        private readonly Mock<IMaterialRepository> materialRepositoryMock;
        private readonly Mock<IIISUnitOfWork> unitOfWorkMock;
        private readonly Mock<IIISUnitOfWorkFactory> unitOfWorkFactoryMock;

        public MaterialUpdateTests()
        {
            materialRepositoryMock = new Mock<IMaterialRepository>(MockBehavior.Strict);
            _serviceProvider = Utils.GetServiceProviderWithCustomSetup(
                serviceCollection =>
                {
                    serviceCollection.AddTransient(_ => materialRepositoryMock.Object);
                });

            unitOfWorkMock = new Mock<IIISUnitOfWork>();
            unitOfWorkFactoryMock = new Mock<IIISUnitOfWorkFactory>();

            var configurationMock = new Mock<IConfiguration>();
            configurationMock.Setup(c => c.GetSection(It.IsAny<String>())).Returns(new Mock<IConfigurationSection>().Object);

            unitOfWorkFactoryMock.Setup(x => x.Create()).Returns(unitOfWorkMock.Object);
            unitOfWorkMock.Setup(x => x.MaterialRepository).Returns(materialRepositoryMock.Object);
            materialProvider = new MaterialProvider<IIISUnitOfWork>(new Mock<IOntologyService>().Object,
                new Mock<IOntologySchema>().Object,
                new Mock<IOntologyNodesData>().Object,
                new Mock<IMaterialElasticService>().Object,
                new Mock<IMLResponseRepository>().Object,
                new Mock<IMaterialSignRepository>().Object,
                new Mock<IMapper>().Object,
                unitOfWorkFactoryMock.Object,
                configurationMock.Object,
                new Mock<IHttpClientFactory>().Object,
                new Api.Ontology.NodeToJObjectMapper(new Mock<IOntologyService>().Object, new FileUrlGetter(new Mock<IHttpContextAccessor>().Object)));
        }
        public void Dispose()
        {
            var context = _serviceProvider.GetRequiredService<OntologyContext>();

            context.MaterialSigns.RemoveRange(context.MaterialSigns);
            context.MaterialSignTypes.RemoveRange(context.MaterialSignTypes);

            context.SaveChanges();
        }


        [Theory, RecursiveAutoData]
        public async Task GetMaterialEntitiesAsync_returns_list_of_material_entities(
            [Frozen]List<MaterialEntity> materialEntities)
        {
            materialRepositoryMock.Setup(_ => _.GetAllAsync()).ReturnsAsync(materialEntities);
            var result = await materialProvider.GetMaterialEntitiesAsync();
            Assert.Equal(materialEntities, result);
        }

        //[Theory, RecursiveAutoData]
        //public async Task UpdateAssigneeId(
        //    MaterialEntity material,
        //    UserEntity assignee)
        //{
        //    //arrange
        //    var context = _serviceProvider.GetRequiredService<OntologyContext>();
        //    context.Add(assignee);
        //    context.Add(material);
        //    material.Data = material.Metadata = material.LoadData = null;
        //    material.MaterialInfos = new List<MaterialInfoEntity>();
        //    context.SaveChanges();

        //    material.AssigneeId = assignee.Id;
        //    material.Assignee = null;
        //    material.File = null;

        //    materialRepositoryMock
        //        .Setup(m => m.PutMaterialToElasticSearchAsync(material.Id, It.IsAny<CancellationToken>()))
        //        .ReturnsAsync(true);

        //    materialRepositoryMock
        //        .Setup(m => m.GetByIdAsync(material.Id, It.IsAny<MaterialIncludeEnum[]>()))
        //        .ReturnsAsync(material);

        //    //act
        //    var sut = _serviceProvider.GetRequiredService<IMaterialService>();
        //    await sut.UpdateMaterialAsync(new MaterialUpdateInput
        //    {
        //        AssigneeId = material.AssigneeId,
        //        CompletenessId = material.CompletenessSignId,
        //        Id = material.Id,
        //        ImportanceId = material.ImportanceSignId,
        //        RelevanceId = material.RelevanceSignId,
        //        ReliabilityId = material.ReliabilitySignId,
        //        SessionPriorityId = material.SessionPriorityId,
        //        SourceReliabilityId = material.SourceReliabilitySignId,
        //        Title = material.Title
        //    });

        //    //assert
        //    var res = context.Materials.First(p => p.Id == material.Id);
        //    Assert.Equal(assignee.Id, res.AssigneeId);
        //    materialRepositoryMock
        //        .Verify(m => m.PutMaterialToElasticSearchAsync(material.Id, It.IsAny<CancellationToken>()));
        //}

        //[Theory, RecursiveAutoData]
        //public async Task Update_ContentIsNull_DoesNotUpdate(MaterialEntity material)
        //{
        //    //arrange
        //    var context = _serviceProvider.GetRequiredService<OntologyContext>();
        //    context.Add(material);
        //    material.Data = material.Metadata = material.LoadData = null;
        //    material.MaterialInfos = new List<MaterialInfoEntity>();
        //    context.SaveChanges();

        //    materialRepositoryMock
        //        .Setup(m => m.PutMaterialToElasticSearchAsync(material.Id, It.IsAny<CancellationToken>()))
        //        .ReturnsAsync(true);

        //    materialRepositoryMock
        //        .Setup(m => m.GetByIdAsync(material.Id, It.IsAny<MaterialIncludeEnum[]>()))
        //        .ReturnsAsync(material);

        //    //act
        //    var sut = _serviceProvider.GetRequiredService<IMaterialService>();
        //    await sut.UpdateMaterialAsync(new MaterialUpdateInput {
        //        Id = material.Id,
        //        Title = "UpdatedTitle"
        //    });

        //    //assert
        //    var provider = _serviceProvider.GetRequiredService<IMaterialProvider>();
        //    var res = await provider.GetMaterialAsync(material.Id);
        //    Assert.Equal(material.Content, res.Content);
        //    materialRepositoryMock
        //        .Verify(m => m.PutMaterialToElasticSearchAsync(material.Id, It.IsAny<CancellationToken>()));
        //}

        //[Theory, RecursiveAutoData]
        //public async Task Update_IsEmptyString_Updated(MaterialEntity material)
        //{
        //    //arrange
        //    var context = _serviceProvider.GetRequiredService<OntologyContext>();
        //    context.Add(material);
        //    material.Data = material.Metadata = material.LoadData = null;
        //    material.MaterialInfos = new List<MaterialInfoEntity>();
        //    context.SaveChanges();

        //    materialRepositoryMock
        //        .Setup(m => m.PutMaterialToElasticSearchAsync(material.Id, It.IsAny<CancellationToken>()))
        //        .ReturnsAsync(true);

        //    material.Content = string.Empty;
        //    materialRepositoryMock
        //        .Setup(m => m.GetByIdAsync(material.Id, It.IsAny<MaterialIncludeEnum[]>()))
        //        .ReturnsAsync(material);

        //    //act
        //    var sut = _serviceProvider.GetRequiredService<IMaterialService>();
        //    await sut.UpdateMaterialAsync(new MaterialUpdateInput
        //    {
        //        Id = material.Id,
        //        Title = "UpdatedTitle",
        //        Content = string.Empty
        //    });

        //    //assert
        //    var provider = _serviceProvider.GetRequiredService<IMaterialProvider>();
        //    var res = await provider.GetMaterialAsync(material.Id);
        //    Assert.Equal(string.Empty, res.Content);
        //    materialRepositoryMock
        //        .Verify(m => m.PutMaterialToElasticSearchAsync(material.Id, It.IsAny<CancellationToken>()));
        //}

        //[Theory, RecursiveAutoData]
        //public async Task UpdateSessionPriority_ReturnsSessionPriorityBack(
        //    MaterialEntity materialEntity,
        //    MaterialSignTypeEntity typeEntity,
        //    MaterialSignEntity important
        //    )
        //{
        //    //arrange
        //    var context = _serviceProvider.GetRequiredService<OntologyContext>();

        //    typeEntity.Name = "Session Priority";
        //    typeEntity.Title = "Пріоритет сесії";
        //    typeEntity.MaterialSigns = null;

        //    important.MaterialSignType = null;
        //    important.MaterialSignTypeId = typeEntity.Id;
        //    important.OrderNumber = 1;
        //    important.ShortTitle = "1";
        //    important.Title = "Перша категорія";

        //    materialEntity.File = null;
        //    materialEntity.FileId = null;

        //    materialEntity.Data = null;
        //    materialEntity.Metadata = null;
        //    materialEntity.LoadData = null;
        //    materialEntity.MaterialInfos = null;

        //    materialEntity.SessionPriority = null;
        //    materialEntity.SessionPriority = null;

        //    context.MaterialSignTypes.Add(typeEntity);
        //    context.MaterialSigns.Add(important);
        //    context.Materials.Add(materialEntity);

        //    await context.SaveChangesAsync();

        //    materialRepositoryMock
        //        .Setup(m => m.PutMaterialToElasticSearchAsync(materialEntity.Id, It.IsAny<CancellationToken>()))
        //        .ReturnsAsync(true);

        //    materialEntity.SessionPriority = new MaterialSignEntity
        //    {
        //        Id = important.Id,
        //        MaterialSignTypeId = typeEntity.Id
        //    };
        //    materialRepositoryMock
        //        .Setup(m => m.GetByIdAsync(materialEntity.Id, It.IsAny<MaterialIncludeEnum[]>()))
        //        .ReturnsAsync(materialEntity);

        //    //act
        //    var materialService = _serviceProvider.GetRequiredService<IMaterialService>();
        //    var material = await materialService.UpdateMaterialAsync(new MaterialUpdateInput {
        //        Id = materialEntity.Id,
        //        SessionPriorityId = important.Id
        //    });

        //    //assert
        //    Assert.Equal(important.Id, material.SessionPriority.Id);
        //    materialRepositoryMock
        //        .Verify(m => m.PutMaterialToElasticSearchAsync(material.Id, It.IsAny<CancellationToken>()));
        //}

        //[Theory(DisplayName = "Set status as Оброблено"), RecursiveAutoData]
        //public async Task SetProcessStatusAsProcessed(MaterialSignTypeEntity typeEntity,
        //    MaterialSignEntity processed,
        //    MaterialSignEntity notProcessed,
        //    MaterialEntity materialEntity)
        //{
        //    //arrange:begin
        //    var context = _serviceProvider.GetRequiredService<OntologyContext>();

        //    typeEntity.Name = "ProcessedStatus";
        //    typeEntity.Title = "Обробка";
        //    typeEntity.MaterialSigns = null;

        //    processed.MaterialSignType = null;
        //    processed.MaterialSignTypeId = typeEntity.Id;
        //    processed.OrderNumber = 1;
        //    processed.ShortTitle = "1";
        //    processed.Title = "Оброблено";

        //    notProcessed.MaterialSignType = null;
        //    notProcessed.MaterialSignTypeId = typeEntity.Id;
        //    notProcessed.OrderNumber = 2;
        //    notProcessed.ShortTitle = "2";
        //    notProcessed.Title = "Не оброблено";

        //    materialEntity.File = null;
        //    materialEntity.FileId = null;

        //    materialEntity.Data = null;
        //    materialEntity.Metadata = null;
        //    materialEntity.LoadData = null;
        //    materialEntity.MaterialInfos = null;

        //    materialEntity.Completeness = null;
        //    materialEntity.CompletenessSignId = null;

        //    materialEntity.Importance = null;
        //    materialEntity.ImportanceSignId = null;

        //    materialEntity.Relevance = null;
        //    materialEntity.RelevanceSignId = null;

        //    materialEntity.Reliability = null;
        //    materialEntity.ReliabilitySignId = null;

        //    materialEntity.Parent = null;
        //    materialEntity.ParentId = null;

        //    materialEntity.SourceReliability = null;
        //    materialEntity.SourceReliabilitySignId = null;

        //    materialEntity.ProcessedStatus = null;
        //    materialEntity.ProcessedStatusSignId = notProcessed.Id;

        //    context.MaterialSignTypes.Add(typeEntity);
        //    context.MaterialSigns.Add(processed);
        //    context.MaterialSigns.Add(notProcessed);
        //    context.Materials.Add(materialEntity);

        //    await context.SaveChangesAsync();

        //    var materialProvider = _serviceProvider.GetRequiredService<IMaterialProvider>();
        //    var materialService = _serviceProvider.GetRequiredService<IMaterialService>();


        //    materialRepositoryMock
        //        .Setup(m => m.PutMaterialToElasticSearchAsync(materialEntity.Id, It.IsAny<CancellationToken>()))
        //        .ReturnsAsync(true);

        //    materialEntity.ProcessedStatus = new MaterialSignEntity
        //    {
        //        Id = processed.Id,
        //        MaterialSignTypeId = typeEntity.Id
        //    };
        //    materialRepositoryMock
        //        .Setup(m => m.GetByIdAsync(materialEntity.Id, It.IsAny<MaterialIncludeEnum[]>()))
        //        .ReturnsAsync(materialEntity);
        //    //arrange:end

        //    var material = await materialProvider.GetMaterialAsync(materialEntity.Id);

        //    //assert
        //    await materialService.UpdateMaterialAsync(new MaterialUpdateInput
        //    {
        //        AssigneeId = material.AssigneeId,
        //        CompletenessId = material.CompletenessSignId,
        //        Id = material.Id,
        //        ImportanceId = material.ImportanceSignId,
        //        Objects = material.LoadData.Objects,
        //        ProcessedStatusId = processed.Id,
        //        RelevanceId = material.RelevanceSignId,
        //        ReliabilityId = material.ReliabilitySignId,
        //        SessionPriorityId = material.SessionPriorityId,
        //        SourceReliabilityId = material.SourceReliabilitySignId,
        //        States = material.LoadData.States,
        //        Tags = material.LoadData.Tags,
        //        Title = material.Title
        //    });

        //    material = await materialProvider.GetMaterialAsync(materialEntity.Id);

        //    //assert
        //    Assert.Equal(processed.Id, material.ProcessedStatusSignId.Value);
        //    materialRepositoryMock
        //        .Verify(m => m.PutMaterialToElasticSearchAsync(material.Id, It.IsAny<CancellationToken>()));
        //}
    }
}