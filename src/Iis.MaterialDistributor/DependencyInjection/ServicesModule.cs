using Microsoft.Extensions.DependencyInjection;
using Iis.MaterialDistributor.Contracts.Services;
using Iis.MaterialDistributor.Services;

namespace Iis.MaterialDistributor.DependencyInjection
{
    public static class ServicesModule
    {
        public static IServiceCollection RegisterServices(this IServiceCollection services)
        {
            services.AddTransient<IMaterialService, MaterialService>();
            return services;
        }
    }
}