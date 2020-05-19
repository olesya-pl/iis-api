using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

using Iis.Domain.Materials;
using Iis.Domain.MachineLearning;
using Iis.DataModel.Materials;
using Iis.Interfaces.Elastic;
using Iis.Interfaces.Materials;

namespace IIS.Core.Materials
{
    public interface IMaterialProvider
    {
        Task<Material> GetMaterialAsync(Guid id);
        Task<MaterialEntity> GetMaterialEntityAsync(Guid id);
        Task<(IEnumerable<Material> Materials,
            int Count,
            Dictionary<Guid, SearchByConfiguredFieldsResultItem> Highlights)> GetMaterialsAsync(int limit,
            int offset,
            string filterQuery,
            IEnumerable<Guid> nodeIds = null,
            IEnumerable<string> types = null);
        Task<IEnumerable<MaterialEntity>> GetMaterialEntitiesAsync();
        IReadOnlyCollection<MaterialSignEntity> GetMaterialSigns(string typeName);
        MaterialSign GetMaterialSign(Guid id);
        Task<Material> MapAsync(MaterialEntity material);
        Task<List<MlProcessingResult>> GetMlProcessingResultsAsync(Guid materialId);
        Task<JObject> GetMaterialDocumentAsync(Guid materialId);
        Task<(IEnumerable<Material> Materials, int Count)> GetMaterialsByNodeIdQuery(Guid nodeId);
        Task<List<MaterialsCountByType>> CountMaterialsByTypeAndNodeAsync(Guid nodeId);
    }
}
