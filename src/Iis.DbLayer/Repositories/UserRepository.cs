using System;
using System.Linq;
using System.Linq.Expressions;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using IIS.Repository;
using Iis.DataModel;
using Iis.DataModel.Roles;
using Iis.DataModel.Materials;
using Iis.DbLayer.Extensions;
using Iis.Interfaces.Constants;

namespace Iis.DbLayer.Repositories
{
    internal class UserRepository : RepositoryBase<OntologyContext>, IUserRepository
    {
        public UserEntity GetByUserNameAndHash(string userName, string passwordHash)
        {
            return GetUsersQuery()
                .SingleOrDefault(e => e.Username == userName && e.PasswordHash == passwordHash);
        }

        public Task<UserEntity> GetByUserNameAsync(string userName, CancellationToken cancellationToken = default)
        {
            return GetUsersQuery()
                .SingleOrDefaultAsync(_ => _.Username == userName && !_.IsBlocked, cancellationToken);
        }

        public Task<UserEntity> GetByIdAsync(Guid userId, CancellationToken ct = default)
        {
            return GetUsersQuery()
                .FirstOrDefaultAsync(e => e.Id == userId, ct);
        }

        public Task<List<UserEntity>> GetAllUsersAsync(CancellationToken ct)
        {
            return GetUsersQuery()
                .AsNoTracking()
                .ToListAsync(ct);
        }

        public Task<UserEntity[]> GetOperatorsAsync(CancellationToken ct = default)
        {
            return GetUsersQuery()
                .Where(e => e.UserRoles.Any(r => r.RoleId == UserRoleConstants.OperatorRoleId) && !e.IsBlocked)
                .ToArrayAsync(ct);
        }

        public Task<UserEntity[]> GetUsersAsync(Expression<Func<UserEntity, bool>> predicate, CancellationToken ct = default)
        {
            return GetUsersQuery()
                .Where(e => !e.IsBlocked)
                .Where(predicate)
                .ToArrayAsync(ct);
        }

        public Task<UserEntity[]> GetUsersAsync(
            int skip,
            int take,
            string sortColumn,
            ListSortDirection? sortDirection,
            Expression<Func<UserEntity, bool>> predicate = null,
            CancellationToken cancellationToken = default)
        {
            var query = GetUsersQuery();
            if (predicate != null)
                query = query.Where(predicate);

            return query.OrderBy(sortColumn, sortDirection)
                .Skip(skip)
                .Take(take)
                .ToArrayAsync(cancellationToken);
        }

        public Task<int> GetUserCountAsync(Expression<Func<UserEntity, bool>> predicate = null, CancellationToken cancellationToken = default)
        {
            var query = GetUsersQuery();
            if (predicate != null)
                query = query.Where(predicate);

            return query.CountAsync(cancellationToken);
        }

        public Task<Dictionary<string, IEnumerable<RoleEntity>>> GetRolesByUserNamesDictionaryAsync(ISet<string> userNames, CancellationToken cancellationToken = default)
        {
            return Context.Users
                .AsNoTracking()
                .Include(user => user.UserRoles)
                    .ThenInclude(userRoles => userRoles.Role)
                .Where(user => userNames.Contains(user.Username))
                .Select(user => new
                {
                    UserName = user.Username,
                    Roles = user.UserRoles.Select(userRole => userRole.Role)
                })
                .ToDictionaryAsync(userRoles => userRoles.UserName, userRoles => userRoles.Roles, cancellationToken);
        }

        public Task<MaterialChannelMappingEntity[]> GetAllMaterialChannelMappingAsync(CancellationToken cancellationToken = default)
        {
            return Context.MaterialChannelMappings
                .AsNoTracking()
                .ToArrayAsync(cancellationToken);
        }

        private IQueryable<UserEntity> GetUsersQuery()
        {
            return Context.Users
                .Include(u => u.SecurityLevels)
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .ThenInclude(r => r.RoleAccessEntities)
                .ThenInclude(ra => ra.AccessObject)
                .AsNoTracking();
        }
    }
}