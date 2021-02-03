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
using Iis.Domain;
using Iis.Interfaces.Ontology.Schema;
using Iis.DbLayer.OntologySchema;
using Iis.Services.Contracts.Interfaces;
using Iis.Interfaces.Ontology.Data;

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

    public class Utils
    {
        private static Utils _instance;
        private static Utils Instance => _instance ?? (_instance = CreateInstance());
        public Startup Startup { get; private set; }
        public ServiceCollection ServiceCollection { get; private set; }
        public ServiceProvider ServiceProvider => ServiceCollection.BuildServiceProvider();

        public static ServiceProvider GetServiceProvider()
        {
            return Instance.ServiceProvider;
        }

        public static string GetConnectionString()
        {
            return Instance.Startup.Configuration.GetConnectionString("DB");
        }

        public static IServiceProvider GetServiceProviderWithCustomSetup(Action<ServiceCollection> setup)
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
            serviceCollection.AddSingleton(new Mock<IOntologySchema>().Object);
            serviceCollection.AddSingleton(new Mock<IElasticConfiguration>().Object);
            serviceCollection.AddSingleton(new Mock<IMaterialEventProducer>().Object);
            serviceCollection.AddTransient<IFileService>(factory => new Mock<IFileService>().Object);
            serviceCollection.AddTransient<IOntologyService>(factory => new Mock<IOntologyService>().Object);
            setup(serviceCollection);
            return serviceCollection.BuildServiceProvider();
        }

        private static Utils CreateInstance()
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
            serviceCollection.AddSingleton(new Mock<IOntologySchema>().Object);
            serviceCollection.AddSingleton(new Mock<IElasticConfiguration>().Object);
            serviceCollection.AddSingleton(new Mock<IMaterialEventProducer>().Object);
            serviceCollection.AddTransient<IFileService>(factory => new Mock<IFileService>().Object);
            serviceCollection.AddTransient<IElasticService>(factory => new Mock<IElasticService>().Object);
            serviceCollection.AddTransient<IOntologyService>(factory => new Mock<IOntologyService>().Object);
            serviceCollection.AddTransient<IOntologyNodesData>(factory => new Mock<IOntologyNodesData>().Object);

            return new Utils
            {
                Startup = startup,
                ServiceCollection = serviceCollection
            };
        }

        public static IOntologySchema GetOntologySchemaFromDb(string connectionString)
        {
            var schemaSource = new OntologySchemaSource
            {
                Title = "DB",
                SourceKind = SchemaSourceKind.Database,
                Data = connectionString
            };
            return (new OntologySchemaService()).GetOntologySchema(schemaSource);
        }
        public static IOntologySchema GetEmptyOntologySchema()
        {
            var schemaSource = new OntologySchemaSource
            {
                Title = "NEW",
                SourceKind = SchemaSourceKind.New,
                Data = string.Empty
            };
            return (new OntologySchemaService()).GetOntologySchema(schemaSource);
        }
        public static OntologyContext GetContext()
        {
            return Instance.ServiceProvider.GetService<OntologyContext>();
        }
        public static OntologyContext GetRealDbContext()
        {
            return OntologyContext.GetContext(GetConnectionString());
        }
    }
}
