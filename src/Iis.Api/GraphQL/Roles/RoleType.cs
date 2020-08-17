using System.Linq;
using AutoMapper;
using HotChocolate.Types;
using Iis.Api.GraphQL.Roles;
using Iis.Services.Contracts.Interfaces;

namespace IIS.Core.GraphQL.Roles
{
    public class RoleType : ObjectType<Role>
    {
        protected override void Configure(IObjectTypeDescriptor<Role> descriptor)
        {
            descriptor.Field(x => x.ActiveDirectoryGroupIds).Ignore();
            descriptor.Field(x => x.ActiveDirectoryGroups).Resolver(context =>
            {
                var client = context.Service<IActiveDirectoryClient>();
                var mapper = context.Service<IMapper>();
                var groupIds = context.Parent<Role>().ActiveDirectoryGroupIds.ToArray();
                return client.GetGroupsByIds(groupIds).Select(x => mapper.Map<Group>(x));
            });
        }
    }
}