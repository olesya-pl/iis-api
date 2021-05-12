using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Iis.DataModel;
using IIS.Repository;
using Microsoft.EntityFrameworkCore;

namespace Iis.DbLayer.Repositories
{
    internal class UserRepository : RepositoryBase<OntologyContext>, IUserRepository
    {
        public Task<UserEntity> GetByIdAsync(Guid userId)
        {
            return Context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == userId);
        }

        public Task<List<UserEntity>> GetAllUsersAsync(CancellationToken cancellationToken)
        {
            return Context.Users
                .AsNoTracking()
                .ToListAsync(cancellationToken);
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
