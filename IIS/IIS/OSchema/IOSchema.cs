using System.Collections.Generic;
using System.Threading.Tasks;

namespace IIS.OSchema
{
    public interface IOSchema
    {
        Task<TypeEntity> GetRootAsync();

        Task<IEnumerable<Entity>> GetEntitiesAsync(string typeName);
    }
}
