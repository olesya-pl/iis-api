using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using AutoMapper;
using HotChocolate;
using Iis.Api.GraphQL.Roles;
using Iis.Roles;
using ValidationContext = System.ComponentModel.DataAnnotations.ValidationContext;

namespace IIS.Core.GraphQL.Roles
{
    public class Mutation
    {
        public async Task<Role> CreateRole([Service] RoleSaver roleSaver,
            [Service] IMapper mapper,
            [GraphQLNonNullType] CreateRoleModel data) 
        {
            Validator.ValidateObject(data, new ValidationContext(data), true);
            var mapped = mapper.Map<Iis.Roles.Role>(data);
            var tabs = mapper.Map<AccessGrantedList>(data.Tabs);
            var entities = mapper.Map<AccessGrantedList>(data.Entities);
            mapped.AccessGrantedItems = tabs.Merge(entities);
            var role = await roleSaver.CreateRoleAsync(mapped);
            return mapper.Map<Role>(role);
        }
    }
}
