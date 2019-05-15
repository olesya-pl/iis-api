using System.Collections.Generic;
using System.Threading.Tasks;

namespace IIS.OSchema
{
    public interface IOSchema
    {
        Task<TypeEntity> GetRootAsync();

        Task<IDictionary<long, EntityValue>> GetEntitiesByAsync(IEnumerable<long> entityIds);

        Task<IDictionary<string, IEnumerable<EntityValue>>> GetEntitiesAsync(IEnumerable<string> typeNames);
    }
}
