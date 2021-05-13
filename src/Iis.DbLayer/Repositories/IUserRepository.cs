using System;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Iis.DataModel;

namespace Iis.DbLayer.Repositories
{
    public interface IUserRepository
    {
        UserEntity GetByUserNameAndHash(string userName, string passwordHash);
        Task<UserEntity> GetByIdAsync(Guid userId, CancellationToken ct);
        Task<UserEntity> GetByUserNameAndHashAsync(string userName, string passwordHash, CancellationToken ct);
        Task<List<UserEntity>> GetAllUsersAsync(CancellationToken ct);
        Task<UserEntity[]> GetOperatorsAsync(CancellationToken ct);
        Task<UserEntity[]> GetOperatorsAsync(Expression<Func<UserEntity, bool>> predicate, CancellationToken ct);
        Task<UserEntity[]> GetUsersAsync(int skip, int take, Expression<Func<UserEntity, bool>> predicate, CancellationToken ct);
        Task<int> GetUserCountAsync(Expression<Func<UserEntity, bool>> predicate, CancellationToken ct);
    }
}
