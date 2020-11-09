﻿using Iis.DbLayer.Repositories;
using Iis.Services.Contracts.Interfaces;
using Iis.Services.Contracts.Interfaces.Elastic;
using Iis.Services.Elastic;
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

            return services;
        }
    }
}
