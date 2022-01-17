using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Iis.DataModel;
using Iis.DataModel.Materials;
using Iis.UnitTests.TestHelpers;
using IIS.Services.Contracts.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Iis.UnitTests.Materials
{
    public class MaterialProviderTests : IDisposable
    {
        public readonly ServiceProvider _serviceProvider;

        public MaterialProviderTests()
        {
            _serviceProvider = Utils.GetServiceProvider();
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
            Assert.Equal(expected, result.Select(item => (Id: item.Id, Name: item.Title)));
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
                material.MaterialAssignees.Add(new MaterialAssigneeEntity {
                    Assignee = assignee,
                    AssigneeId = assignee.Id
                });
                material.Data = material.Metadata = material.LoadData = null;
                material.MaterialInfos = new List<MaterialInfoEntity>();
            }
            context.AddRange(materialsWithCorrectAssignee);

            materialWithCorrectAssigneeAndParent.MaterialAssignees.Add(new MaterialAssigneeEntity
            {
                Assignee = assignee,
                AssigneeId = assignee.Id
            });
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
                UserTestHelper.AssertUserEntityMappedToUserCorrectly(assignee, item.Assignees.FirstOrDefault(_ => _.Id == assignee.Id));
            }
        }
    }
}
