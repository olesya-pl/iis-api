using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Iis.DataModel;
using Iis.DataModel.Materials;
using Iis.UnitTests.TestHelpers;
using IIS.Core.Materials;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Iis.UnitTests.Materials
{
    public class MaterialProviderTests : IDisposable
    {
        public readonly ServiceProvider _serviceProvider;

        public MaterialProviderTests()
        {
            _serviceProvider = Utils.SetupInMemoryDb();
        }
        public void Dispose()
        {
            var context = _serviceProvider.GetRequiredService<OntologyContext>();

            context.MaterialSigns.RemoveRange(context.MaterialSigns);
            context.MaterialSignTypes.RemoveRange(context.MaterialSignTypes);
            context.Materials.RemoveRange(context.Materials);
            context.SaveChanges();

            _serviceProvider.Dispose();
        }

        [Theory, RecursiveAutoData]
        public async Task GetMaterialsAsync_ReturnsOnlyParentMaterials(List<MaterialEntity> data,
            Guid parentGuid)
        {
            //arrange
            data.Add(new MaterialEntity()
            {
                Id = parentGuid,
                Source = "hf.voice",
                Metadata = "{\"Type\": \"audio\", \"Source\": \"hf.voice\", \"Date\": null, \"Features\": null}"
            });

            var context = _serviceProvider.GetRequiredService<OntologyContext>();
            context.Materials.AddRange(data);
            context.SaveChanges();

            //act
            var sut = _serviceProvider.GetRequiredService<IMaterialProvider>();
            var (items, count, _) = await sut.GetMaterialsAsync(50, 0, null);

            //assert
            Assert.Equal(data.Count(p => p.ParentId == null), count);
            Assert.DoesNotContain(items, p => p.ParentId != null);
            Assert.Contains(items, p => p.Id == parentGuid);
        }

        [Theory, RecursiveAutoData]
        public async Task GetMaterialsAsync_RerurnsAssignee(MaterialEntity data, UserEntity assignee)
        {
            //arrange
            data.Assignee = assignee;
            data.AssigneeId = assignee.Id;
            data.Parent = null;
            data.ParentId = null;
            data.Data = data.Metadata = data.LoadData = null;
            data.MaterialInfos = new List<MaterialInfoEntity>();
            var context = _serviceProvider.GetRequiredService<OntologyContext>();
            context.Materials.AddRange(data);
            context.SaveChanges();

            //act
            var sut = _serviceProvider.GetRequiredService<IMaterialProvider>();
            var (items, _, _) = await sut.GetMaterialsAsync(50, 0, null);

            //assert
            var material = items.First(p => p.Id == data.Id);
            Assert.NotNull(material.Assignee);
            UserTestHelper.AssertUserEntityMappedToUserCorrectly(assignee, material.Assignee);
        }

        [Theory, RecursiveAutoData]
        public async Task CountMaterialsByType_CountOnlyParentMaterials(List<MaterialEntity> data,
            MaterialEntity image1,
            MaterialEntity image2,
            MaterialEntity image3,
            MaterialEntity audio,
            NodeEntity node1,
            NodeEntity node2)
        {
            //arrange
            foreach (var item in data)
            {
                item.Type = "image";
                item.MaterialInfos.First().MaterialFeatures.First().Node = node1;
                item.MaterialInfos.First().MaterialFeatures.First().NodeId = node1.Id;
            }
            image1.Type = "image";
            image1.ParentId = null;
            image1.MaterialInfos.First().MaterialFeatures.First().Node = node1;
            image1.MaterialInfos.First().MaterialFeatures.First().NodeId = node1.Id;
            image2.Type = "image";
            image2.ParentId = null;
            image2.MaterialInfos.First().MaterialFeatures.First().Node = node1;
            image2.MaterialInfos.First().MaterialFeatures.First().NodeId = node1.Id;
            image3.Type = "image";
            image3.ParentId = null;
            image3.MaterialInfos.First().MaterialFeatures.First().Node = node2;
            image3.MaterialInfos.First().MaterialFeatures.First().NodeId = node2.Id;
            audio.Type = "audio";
            audio.ParentId = null;
            audio.MaterialInfos.First().MaterialFeatures.First().Node = node1;
            audio.MaterialInfos.First().MaterialFeatures.First().NodeId = node1.Id;

            var context = _serviceProvider.GetRequiredService<OntologyContext>();
            data.AddRange(new List<MaterialEntity> { image1, image2, image3, audio });
            context.Materials.AddRange(data);
            context.SaveChanges();

            //act
            var sut = _serviceProvider.GetRequiredService<IMaterialProvider>();
            var items = await sut.CountMaterialsByTypeAndNodeAsync(node1.Id);
            var items2 = await sut.CountMaterialsByTypeAndNodeAsync(node2.Id);

            //assert
            Assert.Equal(context.Materials.Count(p => p.Type == "image"
                    && p.ParentId == null
                    && p.MaterialInfos.SelectMany(p => p.MaterialFeatures).Select(p => p.Node).Any(p => p.Id == node1.Id)),
                items.First(p => p.Type == "image").Count);
            Assert.Equal(1, items.First(p => p.Type == "audio").Count);
            Assert.Equal(1, items2.First(p => p.Type == "image").Count);
            Assert.Null(items2.FirstOrDefault(p => p.Type == "audio"));
        }

        [Theory(DisplayName = "Get ProcessedStatus list"), RecursiveAutoData]
        public async Task GetProcessedStatuses(MaterialSignTypeEntity typeEntity,
            MaterialSignEntity processed,
            MaterialSignEntity notProcessed)
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

            context.MaterialSignTypes.Add(typeEntity);
            context.MaterialSigns.Add(processed);
            context.MaterialSigns.Add(notProcessed);

            await context.SaveChangesAsync();

            var materialProvider = _serviceProvider.GetRequiredService<IMaterialProvider>();
            //arrange:end

            //act
            var result = materialProvider.GetMaterialSigns("ProcessedStatus");

            //assert
            var expected = new List<(Guid Id, string Name)>
            {
                (Id : processed.Id, Name: processed.Title),
                (Id : notProcessed.Id, Name: notProcessed.Title)
            };

            Assert.Equal(2, result.Count());
            Assert.Equal(expected,result.Select(item => (Id: item.Id, Name: item.Title)) );
        }

        [Theory, RecursiveAutoData]
        public async Task GetMaterialsByAssigneeIdAsync_ReturnsMaterials_WithGivenAssignee(
            List<MaterialEntity> materialsWithIncorrectAssignee,
            List<MaterialEntity> materialsWithCorrectAssignee,
            MaterialEntity materialWithCorrectAssigneeAndParent,
            UserEntity assignee
        )
        {
            //arrange
            var context = _serviceProvider.GetRequiredService<OntologyContext>();
            context.AddRange(materialsWithIncorrectAssignee);
            context.Add(assignee);

            foreach (var material in materialsWithCorrectAssignee)
            {
                material.Parent = null;
                material.ParentId = null;
                material.Assignee = assignee;
                material.AssigneeId = assignee.Id;
                material.Data = material.Metadata = material.LoadData = null;
                material.MaterialInfos = new List<MaterialInfoEntity>();
            }
            context.AddRange(materialsWithCorrectAssignee);

            materialWithCorrectAssigneeAndParent.Assignee = assignee;
            materialWithCorrectAssigneeAndParent.AssigneeId = assignee.Id;
            materialWithCorrectAssigneeAndParent.Data
                = materialWithCorrectAssigneeAndParent.Metadata
                = materialWithCorrectAssigneeAndParent.LoadData = null;
            materialWithCorrectAssigneeAndParent.MaterialInfos = new List<MaterialInfoEntity>();
            context.Add(materialWithCorrectAssigneeAndParent);

            context.SaveChanges();

            //act
            var sut = _serviceProvider.GetRequiredService<IMaterialProvider>();
            var result = await sut.GetMaterialsByAssigneeIdAsync(assignee.Id);

            //assert
            Assert.Equal(materialsWithCorrectAssignee.Count, result.Count);
            foreach (var item in result.Materials)
            {
                UserTestHelper.AssertUserEntityMappedToUserCorrectly(assignee, item.Assignee);
            }
        }
    }
}
