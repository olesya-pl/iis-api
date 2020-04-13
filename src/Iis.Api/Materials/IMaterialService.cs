using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IIS.Core.GraphQL.Materials;
using Iis.Domain.Materials;
using Material = Iis.Domain.Materials.Material;
using Iis.DataModel.Materials;
using MaterialInfo = Iis.Domain.Materials.MaterialInfo;

namespace IIS.Core.Materials
{
    public interface IMaterialService
    {
        Task SaveAsync(Material material);
        Task SaveAsync(Material material, IEnumerable<IIS.Core.GraphQL.Materials.Node> nodes);
        Task SaveAsync(Guid materialId, MaterialInfo materialInfo);
        Task SaveAsync(MaterialEntity material);
    }
}
