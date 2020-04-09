using AutoMapper;
using HotChocolate;
using HotChocolate.Types;
using Iis.DataModel;
using Iis.Interfaces.Roles;
using Iis.Roles;
using IIS.Core.GraphQL.Common;
using System;
using System.Collections.Generic;
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
    }
}
