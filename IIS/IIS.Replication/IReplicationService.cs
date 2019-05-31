using System.Threading.Tasks;
using IIS.Core;

namespace IIS.Replication
{
    public interface IReplicationService
    {
        void IndexEntity(string message);

        Task CreateIndexAsync(TypeEntity schema);

        Task IndexEntityAsync(Entity entity);
    }
}
