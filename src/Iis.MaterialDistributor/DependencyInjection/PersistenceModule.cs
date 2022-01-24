using Microsoft.Extensions.DependencyInjection;
using Iis.MaterialDistributor.DataModel.Extensions;

namespace Iis.MaterialDistributor.DependencyInjection
{
    public static class PersistenceModule
    {
        private const string ConnectionStringName = "db";

        public static IServiceCollection AddPersistance(this IServiceCollection services)
        {
            services.AddMaterialDistributorContext(ConnectionStringName);

            return services;
        }
    }
}