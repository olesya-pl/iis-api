using System;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Iis.DataModel;
using Iis.DataModel.Roles;

namespace Iis.DbLayer.Repositories
{
    public interface IUserRepository
    {
        UserEntity GetByUserNameAndHash(string userName, string passwordHash);
        UserEntity GetByUserName(string userName);
        Task<UserEntity> GetByIdAsync(Guid userId, CancellationToken ct);
        Task<List<UserEntity>> GetAllUsersAsync(CancellationToken ct);
        Task<UserEntity[]> GetOperatorsAsync(CancellationToken ct = default);
        Task<UserEntity[]> GetOperatorsAsync(Expression<Func<UserEntity, bool>> predicate, CancellationToken ct = default);
        Task<UserEntity[]> GetUsersAsync(int skip, int take, Expression<Func<UserEntity, bool>> predicate, CancellationToken ct);
        Task<int> GetUserCountAsync(Expression<Func<UserEntity, bool>> predicate, CancellationToken ct);
        Task<Dictionary<string, IEnumerable<RoleEntity>>> GetRolesByUserNamesDictionaryAsync(ISet<string> userNames, CancellationToken cancellationToken = default);
    }
}