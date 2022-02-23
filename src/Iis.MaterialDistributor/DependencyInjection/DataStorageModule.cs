using Microsoft.Extensions.DependencyInjection;
using Iis.MaterialDistributor.DataStorage;
using Iis.MaterialDistributor.Contracts.DataStorage;

namespace Iis.MaterialDistributor.DependencyInjection
{
    public static class DataStorageModule
    {
        public static IServiceCollection RegisterDataStorage(this IServiceCollection services)
        {
            services.AddSingleton<IDistributionData, DistributionData>();
            services.AddSingleton<IDistributionDataMediator, DistributionDataMediator>();

            return services;
        }
    }
}
