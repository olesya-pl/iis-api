using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IIS.Core.GraphQL.Materials;

namespace IIS.Core.Materials
{
    public interface IMaterialService
    {
        Task SaveAsync(Materials.Material material);
        Task SaveAsync(Materials.Material material, Guid? parentId);
        Task SaveAsync(Materials.Material material, Guid? parentId, IEnumerable<IIS.Core.GraphQL.Materials.Node> nodes);
        Task SaveAsync(Guid materialId, Materials.MaterialInfo materialInfo);
    }
}
