using System;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Iis.Interfaces.Repository
{
    public interface INodeRepository
    {
        Task<JObject> GetNodeById(Guid id);
    }
}
