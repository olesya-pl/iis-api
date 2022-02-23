using Iis.Api.Authorization.Handlers;
using Iis.Api.Authorization.Requirements;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace Iis.Api.Authorization
{
    public static class AuthorizationExtensions
    {
        public static IServiceCollection AddServiceAuthorization(this IServiceCollection services)
        {
            services.AddAuthorization(_ => _.AddPolicy(Policies.HasGrant, _ => _.RequireAuthenticatedUser().Requirements.Add(new HasGrantAuthorizationRequirement())));

            services.AddSingleton<IAuthorizationHandler, HasGrantAuthorizationHandler>();

            return services;
        }
    }
}