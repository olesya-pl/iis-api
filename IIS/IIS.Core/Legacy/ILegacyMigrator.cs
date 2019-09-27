using System.Threading;
using System.Threading.Tasks;

namespace IIS.Legacy.EntityFramework
{
    public interface ILegacyMigrator
    {
        Task Migrate(CancellationToken cancellationToken = default);
    }
}
