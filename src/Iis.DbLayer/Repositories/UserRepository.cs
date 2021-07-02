using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using IIS.Repository;
using Iis.DataModel;
using Iis.DataModel.Roles;

namespace Iis.DbLayer.Repositories
{
    internal class UserRepository : RepositoryBase<OntologyContext>, IUserRepository
    {
        public UserEntity GetByUserNameAndHash(string userName, string passwordHash)
        {
            return GetUsersQuery()
                .SingleOrDefault(e => e.Username == userName && e.PasswordHash == passwordHash);
        }

        public Task<UserEntity> GetByIdAsync(Guid userId, CancellationToken ct)
        {
            return GetUsersQuery()
                .FirstOrDefaultAsync(e => e.Id == userId, ct);
        }

        public Task<UserEntity> GetByUserNameAndHashAsync(string userName, string passwordHash, CancellationToken ct)
        {
            return GetUsersQuery()
                .SingleOrDefaultAsync(e => e.Username == userName && e.PasswordHash == passwordHash, ct);
        }

        public Task<List<UserEntity>> GetAllUsersAsync(CancellationToken ct)
        {
            return Context.Users
                .AsNoTracking()
                .ToListAsync(ct);
        }

        public Task<UserEntity[]> GetOperatorsAsync(CancellationToken ct)
        {
            return GetUsersQuery()
                .Where(e => e.UserRoles.Any(r => r.RoleId == RoleEntity.OperatorRoleId))
                .ToArrayAsync(ct);
        }

        public Task<UserEntity[]> GetOperatorsAsync(Expression<Func<UserEntity, bool>> predicate, CancellationToken ct)
        {
            return GetUsersQuery()
                .Where(e => e.UserRoles.Any(r => r.RoleId == RoleEntity.OperatorRoleId))
                .Where(predicate)
                .ToArrayAsync(ct);
        }

        public Task<UserEntity[]> GetUsersAsync(int skip, int take, Expression<Func<UserEntity, bool>> predicate, CancellationToken ct)
        {
            return GetUsersQuery()
                .Where(predicate)
                .Skip(skip)
                .Take(take)
                .ToArrayAsync(ct);
        }

        public Task<int> GetUserCountAsync(Expression<Func<UserEntity, bool>> predicate, CancellationToken ct)
        {
            return GetUsersQuery()
                    .CountAsync(predicate, ct);
        }

        private IQueryable<UserEntity> GetUsersQuery()
        {
            return Context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .ThenInclude(r => r.RoleAccessEntities)
                .ThenInclude(ra => ra.AccessObject)
                .AsNoTracking();
        }
    }
}
