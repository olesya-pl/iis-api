using System.Collections.Generic;
using System.Threading.Tasks;

namespace IIS.Core
{
    public interface IOSchema
    {
        Task<TypeEntity> GetRootAsync();

        Task<IDictionary<(long, string), IOntologyNode>> GetEntitiesByAsync(IEnumerable<(long, string)> entityIds);

        Task<IDictionary<string, ArrayRelation>> GetEntitiesAsync(IEnumerable<string> typeNames);
    }
}
