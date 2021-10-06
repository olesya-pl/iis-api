using System.Threading.Tasks;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using AutoMapper;
using HotChocolate;
using ValidationContext = System.ComponentModel.DataAnnotations.ValidationContext;
using Iis.Api.GraphQL.Roles;
using Iis.Services;
using Iis.Domain.Users;

namespace IIS.Core.GraphQL.Roles
{
    public class Mutation
    {
        public async Task<RoleResponse> CreateRole([Service] RoleService roleSaver,
            [Service] IMapper mapper,
            [GraphQLNonNullType] CreateRoleModel data) 
        {
            Validator.ValidateObject(data, new ValidationContext(data), true);
            var mapped = mapper.Map<Iis.Domain.Users.Role>(data);
            PopulateAccessGrantedItems(mapper, mapped, data.Tabs, data.Entities);
            var (role, alreadyExists) = await roleSaver.CreateRoleAsync(mapped);

            return new RoleResponse
            {
                Role = mapper.Map<Role>(role),
                AlreadyExists = alreadyExists
            };
        }

        public async Task<RoleResponse> UpdateRole([Service] RoleService roleSaver,
            [Service] IMapper mapper,
            [GraphQLNonNullType] UpdateRoleModel data)
        {
            Validator.ValidateObject(data, new ValidationContext(data), true);
            var mapped = mapper.Map<Iis.Domain.Users.Role>(data);
            PopulateAccessGrantedItems(mapper, mapped, data.Tabs, data.Entities);
            var (role, alreadyExists) = await roleSaver.UpdateRoleAsync(mapped);

            return new RoleResponse
            {
                Role = mapper.Map<Role>(role),
                AlreadyExists = alreadyExists
            };
        }

        private void PopulateAccessGrantedItems(
            IMapper mapper,
            Iis.Domain.Users.Role role, 
            IEnumerable<AccessTab> tabs,
            IEnumerable<AccessEntity> entities)
        {
            var mappedTabs = mapper.Map<AccessGrantedList>(tabs);
            var mappedEntities = mapper.Map<AccessGrantedList>(entities);
            role.AccessGrantedItems = mappedTabs.Merge(mappedEntities);
        }
    }
}