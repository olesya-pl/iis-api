using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Iis.Interfaces.Enums;
using Iis.Services.Contracts;
using Iis.Services.Contracts.Enums;
using Iis.Services.Contracts.Params;

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
        Task<(IReadOnlyCollection<User> Users, int TotalCount)> GetUsersByStatusAsync(PaginationParams page, UserStatusType userStatusFilter, CancellationToken ct = default);
        Task<User> RejectRole(Guid userId, Guid roleId);
        bool IsAccessLevelAllowedForUser(int userAccessLevel, int newAccessLevel);
        Task<Guid> UpdateUserAsync(User updatedUser, CancellationToken cancellationToken = default);
        Task PutAllUsersToElasticSearchAsync(CancellationToken cancellationToken);
    }
}