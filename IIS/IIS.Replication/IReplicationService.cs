using System.Threading.Tasks;
using IIS.Core;

namespace IIS.Replication
{
    public interface IReplicationService
    {
        Task CreateIndexAsync(TypeEntity schema);
    }
}
