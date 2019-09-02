using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IIS.Core.Materials
{
    public interface IMaterialService
    {
        Task SaveAsync(Materials.Material material);
        Task SaveAsync(Materials.Material material, Guid? parentId);
        Task<Materials.Material> GetMaterialAsync(Guid id);
        Task<IEnumerable<Materials.Material>> GetMaterialsAsync();
    }
}
