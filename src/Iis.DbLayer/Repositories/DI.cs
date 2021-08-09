using Iis.DbLayer.Repositories.Helpers;
using Microsoft.Extensions.DependencyInjection;

namespace Iis.DbLayer.Repositories
{
    public static class DI
    {
        public static void RegisterRepositories(this IServiceCollection services)
        {
            services.AddTransient<IMLResponseRepository, MLResponseRepository>();
            services.AddTransient<IMaterialRepository, MaterialRepository>();
            services.AddTransient<IMaterialSignRepository, MaterialSignRepository>();
            services.AddTransient<IAnnotationsRepository, AnnotationsRepository>();
            services.AddTransient<IFlightRadarRepository, FlightRadarRepository>();
            services.AddTransient<IReportRepository, ReportRepository>();
            services.AddTransient<IElasticFieldsRepository, ElasticFieldsRepository>();
            services.AddTransient<IFileRepository, FileRepository>();
            services.AddTransient<IAliasRepository, AliasRepository>();
            services.AddTransient<IThemeRepository, ThemeRepository>();
            services.AddTransient<IChangeHistoryRepository, ChangeHistoryRepository>();
            services.AddTransient<IUserRepository, UserRepository>();
            services.AddTransient<ITowerLocationRepository, TowerLocationRepository>();
            services.AddTransient<INodeMaterialRelationRepository, NodeMaterialRelationRepository>();
            services.AddTransient<ILocationHistoryRepository, LocationHistoryRepository>();
        }
    }
}
