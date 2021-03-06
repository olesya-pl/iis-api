using System.Threading.Tasks;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using AutoMapper;
using HotChocolate;
using ValidationContext = System.ComponentModel.DataAnnotations.ValidationContext;
using Iis.Api.GraphQL.Roles;
using Iis.Services;
using Iis.Services.Contracts;


namespace IIS.Core.GraphQL.Roles
{
    public class Mutation
    {
        public async Task<Role> CreateRole([Service] RoleService roleSaver,
            [Service] IMapper mapper,
            [GraphQLNonNullType] CreateRoleModel data) 
        {
            Validator.ValidateObject(data, new ValidationContext(data), true);
            var mapped = mapper.Map<Iis.Services.Contracts.Role>(data);
            PopulateAccessGrantedItems(mapper, mapped, data.Tabs, data.Entities);
            var role = await roleSaver.CreateRoleAsync(mapped);
            return mapper.Map<Role>(role);
        }

        public async Task<Role> UpdateRole([Service] RoleService roleSaver,
            [Service] IMapper mapper,
            [GraphQLNonNullType] UpdateRoleModel data)
        {
            Validator.ValidateObject(data, new ValidationContext(data), true);
            var mapped = mapper.Map<Iis.Services.Contracts.Role>(data);
            PopulateAccessGrantedItems(mapper, mapped, data.Tabs, data.Entities);
            var role = await roleSaver.UpdateRoleAsync(mapped);
            return mapper.Map<Role>(role);
        }

        private void PopulateAccessGrantedItems(
            IMapper mapper,
            Iis.Services.Contracts.Role role, 
            IEnumerable<AccessTab> tabs,
            IEnumerable<AccessEntity> entities)
        {
            var mappedTabs = mapper.Map<AccessGrantedList>(tabs);
            var mapperEntities = mapper.Map<AccessGrantedList>(entities);
            role.AccessGrantedItems = mappedTabs.Merge(mapperEntities);
        }
    }
}
