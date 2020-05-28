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

namespace Iis.UnitTests.Materials
{
    public class MaterialUpdateTests : IDisposable
    {
        private readonly ServiceProvider _serviceProvider;

        public MaterialUpdateTests()
        {
            _serviceProvider = Utils.SetupInMemoryDb();
        }
        public void Dispose()
        {
            var context = _serviceProvider.GetRequiredService<OntologyContext>();

            context.MaterialSigns.RemoveRange(context.MaterialSigns);
            context.MaterialSignTypes.RemoveRange(context.MaterialSignTypes);

            context.SaveChanges();

            _serviceProvider.Dispose();
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
            //act
            var sut = _serviceProvider.GetRequiredService<IMaterialService>();
            await sut.UpdateMaterial(new MaterialUpdateInput
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

            //act
            var materialService = _serviceProvider.GetRequiredService<IMaterialService>();
            var material = await materialService.UpdateMaterial(new MaterialUpdateInput {
                Id = materialEntity.Id,
                SessionPriorityId = important.Id
            });

            //assert
            Assert.Equal(important.Id, material.SessionPriority.Id);
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
            //arrange:end

            var material = await materialProvider.GetMaterialAsync(materialEntity.Id);

            //assert
            Assert.Equal(notProcessed.Id, material.ProcessedStatusSignId.Value);

            await materialService.UpdateMaterial(new MaterialUpdateInput
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
        }
    }
}