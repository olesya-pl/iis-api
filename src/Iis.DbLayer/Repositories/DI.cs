using Iis.DbLayer.Repositories.Helpers;
using Iis.Domain.Materials;
using Microsoft.Extensions.DependencyInjection;

namespace Iis.DbLayer.Repositories
{
    public static class DI
    {
        public static void RegisterRepositories(this IServiceCollection services)
        {
            services.AddTransient<NodeFlattener>();
            services.AddTransient<INodeRepository, NodeRepository>();
            services.AddTransient<IMLResponseRepository, MLResponseRepository>();
            services.AddTransient<IMaterialRepository, MaterialRepository>();
            services.AddTransient<IMaterialSignRepository, MaterialSignRepository>();
            services.AddTransient<IAnnotationsRepository, AnnotationsRepository>();
        }
    }
}
