using System.Linq;
using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.Xunit2;
using Iis.DataModel;
using IIS.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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
                options => options.UseInMemoryDatabase("db"),
                ServiceLifetime.Transient);
            startup.RegisterServices(serviceCollection, false);

            var serviceProvider = serviceCollection.BuildServiceProvider();
            return serviceProvider;
        }
    }
}
