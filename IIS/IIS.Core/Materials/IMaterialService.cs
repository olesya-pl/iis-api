using System;
using System.Threading.Tasks;

namespace IIS.Core.Materials
{
    public interface IMaterialService
    {
        Task SaveAsync(Materials.Material material);
        Task SaveAsync(Materials.Material material, Guid? parentId);
        Task SaveAsync(Guid materialId, Materials.MaterialInfo materialInfo);
    }
}