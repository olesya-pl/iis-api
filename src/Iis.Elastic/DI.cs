using Iis.Interfaces.Elastic;
using Microsoft.Extensions.DependencyInjection;
using Iis.Elastic.SearchResult;
namespace Iis.Elastic
{
    public static class DI
    {
        public static void RegisterElasticModules(this IServiceCollection services)
        {
            services.AddSingleton<ElasticLogUtils>();
            services.AddTransient<SearchResultExtractor>();
            services.AddTransient<IElasticManager, ElasticManager>();
        }
    }
}
