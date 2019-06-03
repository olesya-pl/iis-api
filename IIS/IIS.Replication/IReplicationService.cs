using System.Threading.Tasks;
using IIS.Core;

namespace IIS.Replication
{
    public interface IReplicationService
    {
        void IndexEntity(string message);

        Task IndexEntityAsync(Entity entity);
    }
}
