using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Iis.Domain.Users;
using Iis.Services.Contracts.Enums;
using Iis.Services.Contracts.Params;

namespace Iis.Services.Contracts.Interfaces
{
    public interface IUserService
    {
        Task<User> AssignRole(Guid userId, Guid roleId);
        Task<Guid> CreateUserAsync(User newUser);
        Task<List<Guid>> GetAvailableOperatorIdsAsync();
        Task<List<User>> GetOperatorsAsync(CancellationToken ct = default);
        User GetUser(Guid userId);
        User GetUser(string userName, string passwordHash);
        User GetUserByUserName(string userName);
        Task<User> GetUserAsync(Guid userId);
        Task<(IReadOnlyCollection<User> Users, int TotalCount)> GetUsersByStatusAsync(PaginationParams page, UserStatusType userStatusFilter, CancellationToken ct = default);
        Task<User> RejectRole(Guid userId, Guid roleId);
        bool IsAccessLevelAllowedForUser(int userAccessLevel, int newAccessLevel);
        Task<Guid> UpdateUserAsync(User updatedUser, CancellationToken cancellationToken = default);
        Task PutAllUsersToElasticSearchAsync(CancellationToken cancellationToken);
        bool ValidateCredentials(string userName, string password);
        User ValidateAndGetUser(string username, string password);
        string GetPasswordHashAsBase64String(string password);
        string ImportUsersFromExternalSource(IEnumerable<string> userNames = null);
        Task<string> GetUserMatrixInfoAsync();
        Task<string> CreateMatrixUsersAsync(List<string> userNames = null);
    }
}