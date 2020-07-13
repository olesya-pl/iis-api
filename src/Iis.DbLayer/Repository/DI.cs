using Iis.Interfaces.Repository;
using Microsoft.Extensions.DependencyInjection;

namespace Iis.DbLayer.Repository
{
    public static class DI
    {
        public static void RegisterRepositories(this IServiceCollection services)
        {
            services.AddTransient<NodeFlattener>();
            services.AddTransient<INodeRepository, NodeRepository>();
        }
    }
}
