using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Iis.Domain.Materials;
using Iis.Domain.MachineLearning;
using Iis.DataModel.Materials;
using Iis.Interfaces.Elastic;

namespace IIS.Core.Materials
{
    public interface IMaterialProvider
    {
        Task<Material> GetMaterialAsync(Guid id);

        Task<(IEnumerable<Material> Materials,
            int Count,
            Dictionary<Guid, SearchByConfiguredFieldsResultItem> Highlights)> GetMaterialsAsync(int limit,
            int offset,
            string filterQuery,
            IEnumerable<Guid> nodeIds = null,
            IEnumerable<string> types = null,
            string sortColumnName = null,
            string order = null);
        Task<IEnumerable<MaterialEntity>> GetMaterialEntitiesAsync();
        IReadOnlyCollection<MaterialSignEntity> GetMaterialSigns(string typeName);
        MaterialSign GetMaterialSign(string signValue);
        MaterialSign GetMaterialSign(Guid id);
        Task<Material> MapAsync(MaterialEntity material);
        Task<List<MLResponse>> GetMLProcessingResultsAsync(Guid materialId);
        Task<(IEnumerable<Material> Materials, int Count)> GetMaterialsByNodeIdQuery(Guid nodeId);
        Task<List<MaterialsCountByType>> CountMaterialsByTypeAndNodeAsync(Guid nodeId);
        Task<(List<Material> Materials, int Count)> GetMaterialsByAssigneeIdAsync(Guid assigneeId);
    }
}
