using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using AutoMapper;

using Xunit;
using Iis.Domain.Materials;
using Iis.DataModel;
using Iis.DataModel.Materials;
using IIS.Core.Materials;
using IIS.Core.GraphQL.Materials;
using IIS.Core;
using Microsoft.Extensions.Configuration;
using Iis.DbLayer.Repositories;
using Moq;
using IIS.Core.Files;
using System.Threading;
using Iis.DbLayer.MaterialEnum;

namespace Iis.UnitTests.Materials
{
    public class MaterialUpdateTests : IDisposable
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly Mock<IMaterialRepository> _materialRepositoryMock;

        public MaterialUpdateTests()
        {
            _materialRepositoryMock = new Mock<IMaterialRepository>(MockBehavior.Strict);
            _serviceProvider = Utils.GetServiceProviderWithCustomSetup(
                serviceCollection => {
                    serviceCollection.AddTransient(_ => _materialRepositoryMock.Object);
                });
        }
        public void Dispose()
        {
            var context = _serviceProvider.GetRequiredService<OntologyContext>();

            context.MaterialSigns.RemoveRange(context.MaterialSigns);
            context.MaterialSignTypes.RemoveRange(context.MaterialSignTypes);

            context.SaveChanges();
        }

        [Theory, RecursiveAutoData]
        public async Task UpdateAssigneeId(
            MaterialEntity material,
            UserEntity assignee)
        {
            //arrange
            var context = _serviceProvider.GetRequiredService<OntologyContext>();
            context.Add(assignee);
            context.Add(material);
            material.Data = material.Metadata = material.LoadData = null;
            material.MaterialInfos = new List<MaterialInfoEntity>();
            context.SaveChanges();

            material.AssigneeId = assignee.Id;
            material.Assignee = null;
            material.File = null;

            _materialRepositoryMock
                .Setup(m => m.PutMaterialToElasticSearchAsync(material.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            _materialRepositoryMock
                .Setup(m => m.GetByIdAsync(material.Id, It.IsAny<MaterialIncludeEnum[]>()))
                .ReturnsAsync(material);

            //act
            var sut = _serviceProvider.GetRequiredService<IMaterialService>();
            await sut.UpdateMaterialAsync(new MaterialUpdateInput
            {
                AssigneeId = material.AssigneeId,
                CompletenessId = material.CompletenessSignId,
                Id = material.Id,
                ImportanceId = material.ImportanceSignId,
                RelevanceId = material.RelevanceSignId,
                ReliabilityId = material.ReliabilitySignId,
                SessionPriorityId = material.SessionPriorityId,
                SourceReliabilityId = material.SourceReliabilitySignId,
                Title = material.Title
            });

            //assert
            var res = context.Materials.First(p => p.Id == material.Id);
            Assert.Equal(assignee.Id, res.AssigneeId);
            _materialRepositoryMock
                .Verify(m => m.PutMaterialToElasticSearchAsync(material.Id, It.IsAny<CancellationToken>()));
        }

        [Theory, RecursiveAutoData]
        public async Task Update_ContentIsNull_DoesNotUpdate(MaterialEntity material)
        {
            //arrange
            var context = _serviceProvider.GetRequiredService<OntologyContext>();
            context.Add(material);
            material.Data = material.Metadata = material.LoadData = null;
            material.MaterialInfos = new List<MaterialInfoEntity>();
            context.SaveChanges();

            _materialRepositoryMock
                .Setup(m => m.PutMaterialToElasticSearchAsync(material.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            _materialRepositoryMock
                .Setup(m => m.GetByIdAsync(material.Id, It.IsAny<MaterialIncludeEnum[]>()))
                .ReturnsAsync(material);

            //act
            var sut = _serviceProvider.GetRequiredService<IMaterialService>();
            await sut.UpdateMaterialAsync(new MaterialUpdateInput {
                Id = material.Id,
                Title = "UpdatedTitle"
            });

            //assert
            var provider = _serviceProvider.GetRequiredService<IMaterialProvider>();
            var res = await provider.GetMaterialAsync(material.Id);
            Assert.Equal(material.Content, res.Content);
            _materialRepositoryMock
                .Verify(m => m.PutMaterialToElasticSearchAsync(material.Id, It.IsAny<CancellationToken>()));
        }

        [Theory, RecursiveAutoData]
        public async Task Update_IsEmptyString_Updated(MaterialEntity material)
        {
            //arrange
            var context = _serviceProvider.GetRequiredService<OntologyContext>();
            context.Add(material);
            material.Data = material.Metadata = material.LoadData = null;
            material.MaterialInfos = new List<MaterialInfoEntity>();
            context.SaveChanges();

            _materialRepositoryMock
                .Setup(m => m.PutMaterialToElasticSearchAsync(material.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            material.Content = string.Empty;
            _materialRepositoryMock
                .Setup(m => m.GetByIdAsync(material.Id, It.IsAny<MaterialIncludeEnum[]>()))
                .ReturnsAsync(material);

            //act
            var sut = _serviceProvider.GetRequiredService<IMaterialService>();
            await sut.UpdateMaterialAsync(new MaterialUpdateInput
            {
                Id = material.Id,
                Title = "UpdatedTitle",
                Content = string.Empty
            });

            //assert
            var provider = _serviceProvider.GetRequiredService<IMaterialProvider>();
            var res = await provider.GetMaterialAsync(material.Id);
            Assert.Equal(string.Empty, res.Content);
            _materialRepositoryMock
                .Verify(m => m.PutMaterialToElasticSearchAsync(material.Id, It.IsAny<CancellationToken>()));
        }

        [Theory, RecursiveAutoData]
        public async Task UpdateSessionPriority_ReturnsSessionPriorityBack(
            MaterialEntity materialEntity,
            MaterialSignTypeEntity typeEntity,
            MaterialSignEntity important
            )
        {
            //arrange
            var context = _serviceProvider.GetRequiredService<OntologyContext>();

            typeEntity.Name = "Session Priority";
            typeEntity.Title = "Пріоритет сесії";
            typeEntity.MaterialSigns = null;

            important.MaterialSignType = null;
            important.MaterialSignTypeId = typeEntity.Id;
            important.OrderNumber = 1;
            important.ShortTitle = "1";
            important.Title = "Перша категорія";

            materialEntity.File = null;
            materialEntity.FileId = null;

            materialEntity.Data = null;
            materialEntity.Metadata = null;
            materialEntity.LoadData = null;
            materialEntity.MaterialInfos = null;

            materialEntity.SessionPriority = null;
            materialEntity.SessionPriority = null;

            context.MaterialSignTypes.Add(typeEntity);
            context.MaterialSigns.Add(important);
            context.Materials.Add(materialEntity);

            await context.SaveChangesAsync();

            _materialRepositoryMock
                .Setup(m => m.PutMaterialToElasticSearchAsync(materialEntity.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            materialEntity.SessionPriority = new MaterialSignEntity
            {
                Id = important.Id,
                MaterialSignTypeId = typeEntity.Id
            };
            _materialRepositoryMock
                .Setup(m => m.GetByIdAsync(materialEntity.Id, It.IsAny<MaterialIncludeEnum[]>()))
                .ReturnsAsync(materialEntity);

            //act
            var materialService = _serviceProvider.GetRequiredService<IMaterialService>();
            var material = await materialService.UpdateMaterialAsync(new MaterialUpdateInput {
                Id = materialEntity.Id,
                SessionPriorityId = important.Id
            });

            //assert
            Assert.Equal(important.Id, material.SessionPriority.Id);
            _materialRepositoryMock
                .Verify(m => m.PutMaterialToElasticSearchAsync(material.Id, It.IsAny<CancellationToken>()));
        }

        [Theory(DisplayName = "Set status as Оброблено"), RecursiveAutoData]
        public async Task SetProcessStatusAsProcessed(MaterialSignTypeEntity typeEntity,
            MaterialSignEntity processed,
            MaterialSignEntity notProcessed,
            MaterialEntity materialEntity)
        {
            //arrange:begin
            var context = _serviceProvider.GetRequiredService<OntologyContext>();

            typeEntity.Name = "ProcessedStatus";
            typeEntity.Title = "Обробка";
            typeEntity.MaterialSigns = null;

            processed.MaterialSignType = null;
            processed.MaterialSignTypeId = typeEntity.Id;
            processed.OrderNumber = 1;
            processed.ShortTitle = "1";
            processed.Title = "Оброблено";

            notProcessed.MaterialSignType = null;
            notProcessed.MaterialSignTypeId = typeEntity.Id;
            notProcessed.OrderNumber = 2;
            notProcessed.ShortTitle = "2";
            notProcessed.Title = "Не оброблено";

            materialEntity.File = null;
            materialEntity.FileId = null;

            materialEntity.Data = null;
            materialEntity.Metadata = null;
            materialEntity.LoadData = null;
            materialEntity.MaterialInfos = null;

            materialEntity.Completeness = null;
            materialEntity.CompletenessSignId = null;

            materialEntity.Importance = null;
            materialEntity.ImportanceSignId = null;

            materialEntity.Relevance = null;
            materialEntity.RelevanceSignId = null;

            materialEntity.Reliability = null;
            materialEntity.ReliabilitySignId = null;

            materialEntity.Parent = null;
            materialEntity.ParentId = null;

            materialEntity.SourceReliability = null;
            materialEntity.SourceReliabilitySignId = null;

            materialEntity.ProcessedStatus = null;
            materialEntity.ProcessedStatusSignId = notProcessed.Id;

            context.MaterialSignTypes.Add(typeEntity);
            context.MaterialSigns.Add(processed);
            context.MaterialSigns.Add(notProcessed);
            context.Materials.Add(materialEntity);

            await context.SaveChangesAsync();

            var materialProvider = _serviceProvider.GetRequiredService<IMaterialProvider>();
            var materialService = _serviceProvider.GetRequiredService<IMaterialService>();


            _materialRepositoryMock
                .Setup(m => m.PutMaterialToElasticSearchAsync(materialEntity.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            materialEntity.ProcessedStatus = new MaterialSignEntity
            {
                Id = processed.Id,
                MaterialSignTypeId = typeEntity.Id
            };
            _materialRepositoryMock
                .Setup(m => m.GetByIdAsync(materialEntity.Id, It.IsAny<MaterialIncludeEnum[]>()))
                .ReturnsAsync(materialEntity);
            //arrange:end

            var material = await materialProvider.GetMaterialAsync(materialEntity.Id);

            //assert
            await materialService.UpdateMaterialAsync(new MaterialUpdateInput
            {
                AssigneeId = material.AssigneeId,
                CompletenessId = material.CompletenessSignId,
                Id = material.Id,
                ImportanceId = material.ImportanceSignId,
                Objects = material.LoadData.Objects,
                ProcessedStatusId = processed.Id,
                RelevanceId = material.RelevanceSignId,
                ReliabilityId = material.ReliabilitySignId,
                SessionPriorityId = material.SessionPriorityId,
                SourceReliabilityId = material.SourceReliabilitySignId,
                States = material.LoadData.States,
                Tags = material.LoadData.Tags,
                Title = material.Title
            });

            material = await materialProvider.GetMaterialAsync(materialEntity.Id);

            //assert
            Assert.Equal(processed.Id, material.ProcessedStatusSignId.Value);
            _materialRepositoryMock
                .Verify(m => m.PutMaterialToElasticSearchAsync(material.Id, It.IsAny<CancellationToken>()));
        }
    }
}