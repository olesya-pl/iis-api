using System;
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
    }
}
