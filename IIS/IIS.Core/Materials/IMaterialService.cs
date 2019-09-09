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
        Task<IEnumerable<Materials.Material>> GetMaterialsAsync(int limit, int offset, Guid? parentId = null, IEnumerable<Guid> nodeIds = null);
        Task SaveAsync(Guid materialId, Materials.MaterialInfo materialInfo);
    }
}
