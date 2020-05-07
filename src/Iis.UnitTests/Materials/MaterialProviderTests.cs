using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Iis.DataModel;
using Iis.DataModel.Materials;
using IIS.Core.Materials;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Iis.UnitTests.Materials
{
    public class MaterialProviderTests
    {
        public readonly ServiceProvider _serviceProvider;

        public MaterialProviderTests()
        {
            _serviceProvider = Utils.SetupInMemoryDb();
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
    }
}
