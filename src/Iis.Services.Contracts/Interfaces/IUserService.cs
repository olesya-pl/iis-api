using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Iis.Domain.Users;
using Iis.Interfaces.Elastic;
using Iis.Interfaces.SecurityLevels;
using Iis.Services.Contracts.Enums;
using Iis.Services.Contracts.Materials.Distribution;

namespace Iis.Services.Contracts.Interfaces
{
    public interface IUserService
    {
        Task<User> AssignRoleAsync(Guid userId, Guid roleId);
        Task<Guid> CreateUserAsync(User newUser);
        Task<List<User>> GetOperatorsAsync(CancellationToken ct = default);
        Task<UserDistributionList> GetOperatorsForMaterialsAsync();
        Task<User> GetUserAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<(IReadOnlyCollection<User> Users, int TotalCount)> GetUsersByStatusAsync(PaginationParams page, SortingParams sorting, string suggestion, UserStatusType userStatusFilter, CancellationToken ct = default);
        Task<User> RejectRole(Guid userId, Guid roleId);
        bool IsAccessLevelAllowedForUser(int userAccessLevel, int newAccessLevel);
        Task<Guid> UpdateUserAsync(User updatedUser, CancellationToken cancellationToken = default);
        Task PutAllUsersToElasticSearchAsync(CancellationToken cancellationToken);
        Task<User> ValidateAndGetUserAsync(string username, string password, CancellationToken cancellationToken = default);
        string GetPasswordHashAsBase64String(string password);
        Task<string> ImportUsersFromExternalSourceAsync(IEnumerable<string> userNames = null, CancellationToken cancellationToken = default);
        Task<string> GetUserMatrixInfoAsync();
        Task<string> CreateMatrixUsersAsync(List<string> userNames = null);
        Task<IReadOnlyList<UserSecurityDto>> GetUserSecurityDtosAsync();
        Task SaveUserSecurityAsync(UserSecurityDto userSecurityDto, CancellationToken cancellationToken = default);
    }
}