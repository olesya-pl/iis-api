using System;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Iis.Interfaces.Repository
{
    public interface INodeRepository
    {
        Task<JObject> GetJsonNodeByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<bool> PutNodeAsync(Guid id, CancellationToken cancellationToken = default);
    }
}
