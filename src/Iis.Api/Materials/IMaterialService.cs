using System.Threading.Tasks;
using System.Collections.Generic;
using Iis.Domain.MachineLearning;
using Material = Iis.Domain.Materials.Material;
using Iis.Interfaces.Materials;
using System;

namespace IIS.Core.Materials
{
    public interface IMaterialService
    {
        Task SaveAsync(Material material);
        Task<MlResponse> SaveMlHandlerResponseAsync(MlResponse response);
        Task<Material> UpdateMaterialAsync(IMaterialUpdateInput input);
        Task<Material> AssignMaterialOperatorAsync(Guid materialId, Guid assigneeId);
    }
}
