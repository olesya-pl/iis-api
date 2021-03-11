using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Iis.Interfaces.Enums;
using Iis.Services.Contracts;
namespace Iis.Services.Contracts.Interfaces
{
    public interface IUserService
    {
        Task<User> AssignRole(Guid userId, Guid roleId);
        Task<Guid> CreateUserAsync(User newUser);
        Task<List<Guid>> GetAvailableOperatorIdsAsync();
        Task<List<User>> GetOperatorsAsync();
        User GetUser(Guid userId);
        User GetUser(string userName, string passwordHash);
        Task<User> GetUserAsync(Guid userId);
        Task<(IEnumerable<User> Users, int TotalCount)> GetUsersAsync(int offset, int pageSize);
        Task<User> RejectRole(Guid userId, Guid roleId);
        bool IsAccessLevelAllowedForUser(AccessLevel userAccessLevel, AccessLevel newAccessLevel);
        Task<Guid> UpdateUserAsync(User updatedUser, CancellationToken cancellationToken = default);
        Task PutAllUsersToElasticSearchAsync(CancellationToken cancellationToken);
    }
}