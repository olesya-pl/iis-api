using Iis.DbLayer.Repositories;
using Iis.Services.Contracts.Interfaces;
using Iis.Services.Contracts.Interfaces.Elastic;
using Iis.Services.Elastic;
using IIS.Services.Contracts.Interfaces;
using IIS.Services.Materials;
using Microsoft.Extensions.DependencyInjection;

namespace Iis.Services.DI
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddServices(this IServiceCollection services) 
        {
            services.AddTransient<IAliasService, AliasService<IIISUnitOfWork>>();
            services.AddScoped<OntologyElasticResponseManager>();
            services.AddScoped<MaterialElasticResponseManager>();
            services.AddTransient<IElasticResponseManagerFactory, ElasticResponseManagerFactory>();
            services.AddTransient<IMaterialElasticService, MaterialElasticService>();
            services.AddTransient<IGsmLocationService, GsmLocationService<IIISUnitOfWork>>();
            services.AddTransient<IUserElasticService, UserElasticService>();
            services.AddTransient<CreateEntityService>();
            services.AddTransient<NodeMaterialRelationService<IIISUnitOfWork>>();
            services.AddTransient<IGraphService, GraphService>();
            services.AddTransient<IImageVectorizer, ImageVectorizer>();
            services.AddTransient<IMaterialProvider, MaterialProvider<IIISUnitOfWork>>();
            services.AddHttpClient<MaterialProvider<IIISUnitOfWork>>();
            services.AddTransient<NodeToJObjectMapper>();

            return services;
        }
    }
}
