using System;
using System.Threading.Tasks;
using Iis.DataModel;

namespace Iis.DbLayer.Repositories
{
    public interface IUserRepository
    {
        Task<UserEntity> GetByIdAsync(Guid assigneeId);
    }
}
