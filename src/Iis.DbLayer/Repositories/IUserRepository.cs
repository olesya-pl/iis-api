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
        Task<UserEntity> GetByIdAsync(Guid assigneeId);
        Task<List<UserEntity>> GetAllUsersAsync(CancellationToken cancellationToken);
        Task<UserEntity[]> GetUsersAsync(int skip, int take, Expression<Func<UserEntity, bool>> predicate, CancellationToken ct);
        Task<int> GetUserCountAsync(Expression<Func<UserEntity, bool>> predicate, CancellationToken ct);
    }
}
