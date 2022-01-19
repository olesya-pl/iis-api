using Microsoft.Extensions.DependencyInjection;
using Iis.MaterialDistributor.Contracts.Repositories;
using Iis.MaterialDistributor.Repositories;

namespace Iis.MaterialDistributor.DependencyInjection
{
    public static class RepositoriesModule
    {
        public static IServiceCollection RegisterRepositories(this IServiceCollection services)
        {
            services.AddTransient<IMaterialElasticRepository, MaterialElasticRepository>();

            return services;
        }
    }
}