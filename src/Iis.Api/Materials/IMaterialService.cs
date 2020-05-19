using System.Threading.Tasks;
using System.Collections.Generic;
using Iis.Domain.MachineLearning;
using Material = Iis.Domain.Materials.Material;
using Iis.Interfaces.Materials;
using System;
using Iis.DataModel.Materials;

namespace IIS.Core.Materials
{
    public interface IMaterialService
    {
        [Obsolete("The only test usage is in test")]
        Task SaveAsync(MaterialEntity material);
        Task SaveAsync(Material material, IEnumerable<GraphQL.Materials.Node> nodes);
        Task<MlResponse> SaveMlHandlerResponseAsync(MlResponse response);
        Task<Material> UpdateMaterial(IMaterialUpdateInput input);
        Task<Material> AssignMaterialOperator(Guid materialId, Guid assigneeId);
    }
}
