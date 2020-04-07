using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.Xunit2;
using Iis.DataModel;
using Iis.DataModel.Materials;
using IIS.Core;
using IIS.Core.Materials;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Iis.UnitTests.Materials
{
    public class RecursiveAutoDataAttribute : AutoDataAttribute
    {
        public RecursiveAutoDataAttribute()
            : base(() => { 
                var fixture = new Fixture();
                fixture.Behaviors.OfType<ThrowingRecursionBehavior>()
                    .ToList().ForEach(b => fixture.Behaviors.Remove(b));
                fixture.Behaviors.Add(new OmitOnRecursionBehavior());
                return fixture;
            }        

            )
        { }
    }

    public class MaterialProviderTests
    {
        public readonly ServiceProvider _serviceProvider;

        public MaterialProviderTests()
        {
            _serviceProvider = SetupInMemoryDb();
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

        private static ServiceProvider SetupInMemoryDb()
        {
            var startup = new Startup(new ConfigurationBuilder()
                            .AddJsonFile("appsettings.json", optional: true)
                            .Build());
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddDbContext<OntologyContext>(
                options => options.UseInMemoryDatabase("db"),
                ServiceLifetime.Transient);
            startup.ConfigureServices(serviceCollection, false);

            var serviceProvider = serviceCollection.BuildServiceProvider();
            return serviceProvider;
        }
    }
}
