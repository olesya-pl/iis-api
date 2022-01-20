using Microsoft.Extensions.DependencyInjection;
using MediatR;

namespace Iis.MaterialDistributor.DependencyInjection
{
    public static class MediatRModule
    {
        public static IServiceCollection RegisterMediatRHandlers(this IServiceCollection services)
        {
            services.AddMediatR(typeof(Startup));

            return services;
        }
    }
}