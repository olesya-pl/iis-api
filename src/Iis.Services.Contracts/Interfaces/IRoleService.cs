using Iis.Domain.Users;
using Iis.Services.Contracts;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Iis.Services
{
    public interface IRoleService
    {
        Task<Role> CreateRoleAsync(Role role);
        Task<Role> GetRoleAsync(Guid id);
        Task<List<Role>> GetRolesAsync();
        Task<Role> UpdateRoleAsync(Role role);
    }
}