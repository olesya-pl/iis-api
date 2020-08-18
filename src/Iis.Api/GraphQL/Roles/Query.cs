using AutoMapper;
using HotChocolate;
using HotChocolate.Types;
using Iis.Api.GraphQL.Roles;
using Iis.Interfaces.Roles;
using Iis.Services;
using Iis.Services.Contracts.Interfaces;
using IIS.Core.GraphQL.Common;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace IIS.Core.GraphQL.Roles
{
    public class Query
    {
        public async Task<GraphQLCollection<Role>> GetRoles([Service] RoleService roleLoader, [Service] IMapper mapper)
        {
            var roles = await roleLoader.GetRolesAsync();
            var rolesQl = roles.Select(r => mapper.Map<Role>(r)).ToList();
            return new GraphQLCollection<Role>(rolesQl, roles.Count);
        }

        public async Task<Role> GetRole([Service] RoleService roleLoader, [Service] IMapper mapper, [GraphQLType(typeof(NonNullType<IdType>))] Guid id)
        {
            var role = await roleLoader.GetRoleAsync(id);
            return mapper.Map<Role>(role);
        }

        public AccessObjectsResponse GetAccessObjects([Service] AccessObjectService accessObjectService, 
            [Service] IMapper mapper)
        {
            var accesses = accessObjectService.GetAccesses();
            return new AccessObjectsResponse
            {
                Entities = accesses
                    .Where(p => p.Category == AccessCategory.Entity)
                    .Select(p => mapper.Map<AccessEntity>(p)),
                Tabs = accesses
                    .Where(p => p.Category == AccessCategory.Tab)
                    .Select(p => mapper.Map<AccessTab>(p))
            };
        }

       
        public GraphQLCollection<Group> GetActiveDirectoryGroups([Service] IActiveDirectoryClient client, [Service] IMapper mapper)
        {
            var groups = client.GetAllGroups();
            return new GraphQLCollection<Group>(groups.Select(mapper.Map<Group>).ToList(), groups.Count);
        }
    }
}
