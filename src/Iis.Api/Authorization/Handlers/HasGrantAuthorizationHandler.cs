using System.Linq;
using System.Threading.Tasks;
using HotChocolate.Resolvers;
using Iis.Api.Authorization.Requirements;
using Iis.Api.GraphQL.Access;
using Iis.Interfaces.Roles;
using IIS.Core.GraphQL;
using Microsoft.AspNetCore.Authorization;

namespace Iis.Api.Authorization.Handlers
{
    public class HasGrantAuthorizationHandler : AuthorizationHandler<HasGrantAuthorizationRequirement, IResolverContext>
    {
        private readonly GraphQLAccessList _graphQLAccessList;

        public HasGrantAuthorizationHandler(GraphQLAccessList graphQLAccessList)
        {
            _graphQLAccessList = graphQLAccessList;
        }

        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            HasGrantAuthorizationRequirement requirement,
            IResolverContext resource)
        {
            var user = resource.GetToken().User;
            var operationName = resource.Operation?.Name?.Value ?? resource.Selection.SyntaxNode.Name.Value;
            var variableValues = resource.Variables.ToDictionary(_ => _.Name.Value, _ => _.Value.Value);
            var graphQLAccessItems = _graphQLAccessList.GetAccessItem(operationName, variableValues);

            foreach (var graphQLAccessItem in graphQLAccessItems)
            {
                if (graphQLAccessItem == null || graphQLAccessItem.Kind == AccessKind.FreeForAll)
                {
                    break;
                }

                if (!user.IsGranted(graphQLAccessItem.Kind, graphQLAccessItem.Operation, AccessCategory.Entity))
                {
                    context.Fail();

                    return Task.CompletedTask;
                }
            }

            context.Succeed(requirement);

            return Task.CompletedTask;
        }
    }
}