using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using HotChocolate;
using HotChocolate.Resolvers;
using HotChocolate.Types;
using Iis.Api.GraphQL.Roles;
using IIS.Core.GraphQL.Common;
using Iis.Domain.Users;
using Iis.Interfaces.Roles;
using Iis.Services;
using Iis.Services.Contracts.Interfaces;
using Iis.Services.Contracts.Interfaces.Roles;
using Group = Iis.Api.GraphQL.Roles.Group;

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
            var res = mapper.Map<Role>(role);
            res.Tabs = res.Tabs.Where(p => !ExistsCorresondingEntity(p, res));
            return res;
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
                    .Where(p => p.Category == AccessCategory.Tab
                     && !ExistsCorespondingEntity(p, accesses))
                    .Select(p => mapper.Map<AccessTab>(p))
            };
        }

        public GraphQLCollection<Group> GetActiveDirectoryGroups([Service] IActiveDirectoryClient client, [Service] IMapper mapper)
        {
            var groups = client.GetAllGroups();
            return new GraphQLCollection<Group>(groups.Select(mapper.Map<Group>).ToList(), groups.Count);
        }

        public async Task<GroupAccess> GetGroupRights(
            IResolverContext resolverContext,
            [Service] IGroupsService groupsService,
            [Service] IMapper mapper,
            [GraphQLType(typeof(NonNullType<IdType>))] Guid id)
        {
            var group = await groupsService.GetAsync(id, resolverContext.RequestAborted);

            return mapper.Map<GroupAccess>(group);
        }

        public async Task<GraphQLCollection<GroupRoles>> GetGroupRoles(
            IResolverContext resolverContext,
            [Service] IGroupsService groupsService,
            [Service] IMapper mapper)
        {
            var groups = await groupsService.GetAllAsync(resolverContext.RequestAborted);
            var groupRoles = mapper.Map<GroupRoles[]>(groups);

            return new GraphQLCollection<GroupRoles>(groupRoles, groupRoles.Length);
        }

        private static bool ExistsCorresondingEntity(AccessTab tab, Role res)
        {
            return res.Entities.Any(e => string.Equals(e.Kind, tab.Kind, StringComparison.Ordinal));
        }

        private static bool ExistsCorespondingEntity(AccessGranted itemToCheck, IReadOnlyCollection<AccessGranted> accesses)
        {
            return accesses.Any(access => itemToCheck.Kind == access.Kind && access.Category == AccessCategory.Entity);
        }
    }
}
