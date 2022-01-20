using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using Iis.MaterialDistributor.AutoMapper;

namespace Iis.MaterialDistributor.DependencyInjection
{
    public static class AutoMapperModule
    {
        public static IServiceCollection RegisterAutoMapperProfiles(this IServiceCollection services)
        {
            services.AddAutoMapper(typeof(AutoMapperProfile));

            return services;
        }
    }
}