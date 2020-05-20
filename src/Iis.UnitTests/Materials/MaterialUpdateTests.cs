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
            var mapper = _serviceProvider.GetRequiredService<IMapper>();
            //arrange:end

            var material = await materialProvider.GetMaterialAsync(materialEntity.Id);

            var entity = mapper.Map<MaterialEntity>(material);

            //assert
            Assert.Equal(notProcessed.Id, material.ProcessedStatusSignId.Value);

            entity.ProcessedStatus = null;
            entity.ProcessedStatusSignId = processed.Id;

            await materialService.SaveAsync(entity);

            material = await materialProvider.GetMaterialAsync(materialEntity.Id);

            //assert
            Assert.Equal(processed.Id, material.ProcessedStatusSignId.Value);
        }
    }
}