using Iis.Domain.Users;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Iis.Services
{
    public interface IRoleService
    {
        Task<(Role Role, bool AlreadyExists)> CreateRoleAsync(Role role);
        Task<Role> GetRoleAsync(Guid id);
        Task<List<Role>> GetRolesAsync();
        Task<(Role Role, bool AlreadyExists)> UpdateRoleAsync(Role role);
    }
}