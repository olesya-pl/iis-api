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
            var (items, count) = await sut.GetMaterialsAsync(50, 0, null);
            
            //assert
            Assert.Equal(data.Count(p => p.ParentId == null), count);
            Assert.DoesNotContain(items, p => p.ParentId != null);
            Assert.Contains(items, p => p.Id == parentGuid);
        }        
    }
}
