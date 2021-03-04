using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
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
    }
}
