using System.Threading.Tasks;
using System.Collections.Generic;
using Iis.DataModel.Materials;
using Iis.Domain.MachineLearning;
using Material = Iis.Domain.Materials.Material;

namespace IIS.Core.Materials
{
    public interface IMaterialService
    {
        Task SaveAsync(Material material);
        Task SaveAsync(Material material, IEnumerable<IIS.Core.GraphQL.Materials.Node> nodes);
        Task SaveAsync(MaterialEntity material);
        Task<MlResponse> SaveMlHandlerResponseAsync(MlResponse response);
    }
}
