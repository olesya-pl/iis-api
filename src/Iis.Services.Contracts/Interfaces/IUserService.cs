using Iis.Services.Contracts;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Iis.Services
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
        Task<Guid> UpdateUserAsync(User updatedUser, CancellationToken cancellationToken = default);
        Task PutAllUsersToElasticSearchAsync(CancellationToken cancellationToken);
    }
}