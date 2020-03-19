using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IIS.Core.GraphQL.Materials;
using Iis.Domain.Materials;
using Material = Iis.Domain.Materials.Material;

namespace IIS.Core.Materials
{
    public interface IMaterialService
    {
        Task SaveAsync(Material material);
        Task SaveAsync(Material material, Guid? parentId);
        Task SaveAsync(Material material, Guid? parentId, IEnumerable<IIS.Core.GraphQL.Materials.Node> nodes);
        Task SaveAsync(Guid materialId, MaterialInfo materialInfo);
    }
}
