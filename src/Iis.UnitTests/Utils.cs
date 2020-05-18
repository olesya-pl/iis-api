using System.Linq;
using AutoMapper;
using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.Xunit2;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using IIS.Core;
using Iis.DataModel;
using Iis.DataModel.Cache;
using Iis.Interfaces.Elastic;
using System;
using IIS.Core.Materials;
using IIS.Core.Files;

namespace Iis.UnitTests
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

    public class AutoMoqDataAttribute : AutoDataAttribute
    {
        public AutoMoqDataAttribute()
            : base(() => {
                return new Fixture().Customize(new AutoMoqCustomization());
            }

            )
        { }
    }

    public static class Utils
    {
        public static ServiceProvider SetupInMemoryDb()
        {
            var startup = new Startup(new ConfigurationBuilder()
                            .AddJsonFile("appsettings.json", optional: true)
                            .Build());
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddDbContext<OntologyContext>(
                options => options.UseInMemoryDatabase(Guid.NewGuid().ToString()),
                ServiceLifetime.Transient);
            startup.RegisterServices(serviceCollection, false);
            serviceCollection.AddSingleton<IOntologyCache, OntologyCache>();
            serviceCollection.AddSingleton(new Mock<IElasticConfiguration>().Object);
            serviceCollection.AddSingleton(new Mock<IMaterialEventProducer>().Object);
            serviceCollection.AddTransient<IFileService>(factory => new Mock<IFileService>().Object);
            serviceCollection.AddTransient<IElasticService>(factory => new Mock<IElasticService>().Object);
            
            var serviceProvider = serviceCollection.BuildServiceProvider();
            return serviceProvider;
        }
    }
}
