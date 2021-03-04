using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Iis.Services.Contracts.Dtos;

namespace Iis.Services.Contracts.Interfaces
{
    public interface IUserElasticService
    {
        Task SaveAllUsersAsync(IReadOnlyCollection<ElasticUserDto> elasticUsers, CancellationToken cancellationToken);
        Task SaveUserAsync(ElasticUserDto elasticUser, CancellationToken cancellationToken);
        Task ClearNonPredefinedUsers(CancellationToken cancellationToken);
    }
}
