using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
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
            Dictionary<Guid, SearchResultItem> Highlights)> GetMaterialsAsync(int limit,
            int offset,
            string filterQuery,
            IEnumerable<string> types = null,
            string sortColumnName = null,
            string order = null);
        Task<IEnumerable<MaterialEntity>> GetMaterialEntitiesAsync();
        IReadOnlyCollection<MaterialSignEntity> GetMaterialSigns(string typeName);
        MaterialSign GetMaterialSign(string signValue);
        MaterialSign GetMaterialSign(Guid id);
        Task<Material> MapAsync(MaterialEntity material);
        Task<List<MLResponse>> GetMLProcessingResultsAsync(Guid materialId);
        Task<(IEnumerable<Material> Materials, int Count)> GetMaterialsByImageAsync(int pageSize, int offset, string name, byte[] content);
        Task<(IEnumerable<Material> Materials, int Count)> GetMaterialsByNodeIdQuery(Guid nodeId);
        Task<Dictionary<Guid, int>> CountMaterialsByNodeIds(HashSet<Guid> nodeIds);
        Task<(IEnumerable<Material> Materials, int Count)> GetMaterialsCommonForEntityAndDescendantsAsync(IEnumerable<Guid> nodeIdList, int limit = 0, int offset = 0, CancellationToken ct = default);
        Task<List<MaterialsCountByType>> CountMaterialsByTypeAndNodeAsync(Guid nodeId);
        Task<(List<Material> Materials, int Count)> GetMaterialsByAssigneeIdAsync(Guid assigneeId);
        Task<(IEnumerable<Material> Materials, int Count)> GetMaterialsLikeThisAsync(Guid materialId, int limit, int offset);
    }
}
