using System.Threading.Tasks;
using HotChocolate.Resolvers;
using Iis.Api.Authorization.Requirements;
using IIS.Core.GraphQL;
using Microsoft.AspNetCore.Authorization;

namespace Iis.Api.Authorization.Handlers
{
    public class HasAdminRightsAuthorizationHandler : AuthorizationHandler<HasGrantAuthorizationRequirement, IResolverContext>
    {
        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            HasGrantAuthorizationRequirement requirement,
            IResolverContext resource)
        {
            var user = resource.GetToken().User;

            if (user.IsAdmin)
            {
                context.Succeed(requirement);
            }
            else
            {
                context.Fail();
            }

            return Task.CompletedTask;
        }
    }
}