using Microsoft.Extensions.DependencyInjection;
using Iis.MaterialDistributor.Contracts.Services;
using Iis.MaterialDistributor.Services;
using Iis.MaterialDistributor.PermanentCoefficients;
using Iis.MaterialDistributor.DataStorage;
using Iis.Interfaces.SecurityLevels;
using Iis.Security.SecurityLevels;
using Iis.MaterialDistributor.Contracts.DataStorage;

namespace Iis.MaterialDistributor.DependencyInjection
{
    public static class DataStorageModule
    {
        public static IServiceCollection RegisterDataStorage(this IServiceCollection services)
        {
            services.AddSingleton<IDistributionData, DistributionData>();
            services.AddSingleton<ISecurityLevelChecker, SecurityLevelChecker>();
            services.AddSingleton<IDistributionDataMediator, DistributionDataMediator>();

            return services;
        }
    }
}
