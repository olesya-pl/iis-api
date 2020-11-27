using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Iis.Domain.Materials;
using Iis.Domain.MachineLearning;
using Iis.DataModel.Materials;
using Iis.Services.Contracts.Params;

namespace IIS.Core.Materials
{
    public interface IMaterialProvider
    {
        Task<Material> GetMaterialAsync(Guid id, Guid userId);

        Task<MaterialsDto> GetMaterialsAsync(int limit,
            int offset,
            string filterQuery,
            SortingParams sorting,
            CancellationToken ct = default);
        Task<IEnumerable<MaterialEntity>> GetMaterialEntitiesAsync();
        IReadOnlyCollection<MaterialSignEntity> GetMaterialSigns(string typeName);
        MaterialSign GetMaterialSign(string signValue);
        MaterialSign GetMaterialSign(Guid id);
        Task<Material> MapAsync(MaterialEntity material);
        Task<List<MLResponse>> GetMLProcessingResultsAsync(Guid materialId);
        Task<(IEnumerable<Material> Materials, int Count)> GetMaterialsByImageAsync(int pageSize, int offset, string name, byte[] content);
        Task<(IEnumerable<Material> Materials, int Count)> GetMaterialsByNodeIdQuery(Guid nodeId);
        Task<(IEnumerable<Material> Materials, int Count)> GetMaterialsCommonForEntitiesAsync(IEnumerable<Guid> nodeIdList, 
            bool includeDescendants, 
            string suggestion, 
            int limit, int offset, 
            SortingParams sorting,
            CancellationToken ct = default);
        Task<Dictionary<Guid, int>> CountMaterialsByNodeIds(HashSet<Guid> nodeIds);
        Task<List<MaterialsCountByType>> CountMaterialsByTypeAndNodeAsync(Guid nodeId);
        Task<(List<Material> Materials, int Count)> GetMaterialsByAssigneeIdAsync(Guid assigneeId);
        Task<(IEnumerable<Material> Materials, int Count)> GetMaterialsLikeThisAsync(Guid materialId, int limit, int offset);
        Task<bool> MaterialExists(Guid value);
    }
}
