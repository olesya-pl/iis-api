using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Iis.Domain.Materials;
using Iis.Domain.MachineLearning;
using Iis.DataModel.Materials;
using Iis.Services.Contracts.Params;
using Iis.Interfaces.Elastic;
using Iis.Services.Contracts;

namespace IIS.Core.Materials
{
    public interface IMaterialProvider
    {
        Task<Material> GetMaterialAsync(Guid id, User user);

        Task<MaterialsDto> GetMaterialsAsync(Guid userId,
            string filterQuery,
            PaginationParams page,
            SortingParams sorting,
            CancellationToken ct = default);
        Task<IEnumerable<MaterialEntity>> GetMaterialEntitiesAsync();
        IReadOnlyCollection<MaterialSignEntity> GetMaterialSigns(string typeName);
        MaterialSign GetMaterialSign(string signValue);
        MaterialSign GetMaterialSign(Guid id);
        Task<Material> MapAsync(MaterialEntity material);
        Task<List<MLResponse>> GetMLProcessingResultsAsync(Guid materialId);
        Task<MaterialsDto> GetMaterialsByImageAsync(Guid userId, PaginationParams page, string fileName, byte[] content);
        Task<(IEnumerable<Material> Materials, int Count)> GetMaterialsByNodeIdQuery(Guid nodeId);
        Task<MaterialsDto> GetMaterialsCommonForEntitiesAsync(Guid userId,
            IEnumerable<Guid> nodeIdList, 
            bool includeDescendants,
            string suggestion,
            PaginationParams page,
            SortingParams sorting,
            CancellationToken ct = default);
        Task<Dictionary<Guid, int>> CountMaterialsByNodeIds(HashSet<Guid> nodeIds);
        Task<List<MaterialsCountByType>> CountMaterialsByTypeAndNodeAsync(Guid nodeId);
        Task<(List<Material> Materials, int Count)> GetMaterialsByAssigneeIdAsync(Guid assigneeId);
        Task<(IEnumerable<Material> Materials,  int Count)> GetMaterialsLikeThisAsync(Guid userId, Guid materialId, PaginationParams page, SortingParams sorting);
        Task<bool> MaterialExists(Guid value);
    }
}
