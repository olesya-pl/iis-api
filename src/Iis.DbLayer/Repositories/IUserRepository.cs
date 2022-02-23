using System;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Iis.DataModel;
using Iis.DataModel.Roles;
using Iis.DataModel.Materials;
using System.ComponentModel;

namespace Iis.DbLayer.Repositories
{
    public interface IUserRepository
    {
        UserEntity GetByUserNameAndHash(string userName, string passwordHash);
        Task<UserEntity> GetByUserNameAsync(string userName, CancellationToken cancellationToken = default);
        Task<UserEntity> GetByIdAsync(Guid userId, CancellationToken ct = default);
        Task<List<UserEntity>> GetAllUsersAsync(CancellationToken ct);
        Task<UserEntity[]> GetOperatorsAsync(CancellationToken ct = default);
        Task<UserEntity[]> GetUsersAsync(Expression<Func<UserEntity, bool>> predicate, CancellationToken ct = default);
        Task<UserEntity[]> GetUsersAsync(int skip, int take, string sortColumn, ListSortDirection? sortDirection, Expression<Func<UserEntity, bool>> predicate = null, CancellationToken cancellationToken = default);
        Task<int> GetUserCountAsync(Expression<Func<UserEntity, bool>> predicate = null, CancellationToken cancellationToken = default);
        Task<Dictionary<string, IEnumerable<RoleEntity>>> GetRolesByUserNamesDictionaryAsync(ISet<string> userNames, CancellationToken cancellationToken = default);
        Task<MaterialChannelMappingEntity[]> GetAllMaterialChannelMappingAsync(CancellationToken cancellationToken = default);
    }
}