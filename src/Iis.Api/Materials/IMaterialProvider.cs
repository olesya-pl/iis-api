using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

using Iis.Domain.Materials;
using Iis.Domain.MachineLearning;
using Iis.DataModel.Materials;
using Iis.Interfaces.Materials;
using System.Linq;

namespace IIS.Core.Materials
{
    public interface IMaterialProvider
    {
        Task<Material> GetMaterialAsync(Guid id);
        Task<MaterialEntity> GetMaterialEntityAsync(Guid id);
        Task<(IEnumerable<Material> Materials, int Count)> GetMaterialsAsync(int limit, 
            int offset,
            string filterQuery,
            IEnumerable<Guid> nodeIds = null,
            IEnumerable<string> types = null);
        Task<IEnumerable<MaterialEntity>> GetMaterialEntitiesAsync();
        IReadOnlyCollection<MaterialSignEntity> GetMaterialSigns(string typeName);
        MaterialSign GetMaterialSign(Guid id);
        Task<MaterialEntity> UpdateMaterial(IMaterialUpdateInput input);
        Task<Material> MapAsync(MaterialEntity material);
        Task<List<MlProcessingResult>> GetMlProcessingResultsAsync(Guid materialId);
        Task<JObject> GetMaterialDocumentAsync(Guid materialId);
        Task<(IEnumerable<Material> Materials, int Count)> GetMaterialsByNodeIdQuery(Guid nodeId);
    }
}
