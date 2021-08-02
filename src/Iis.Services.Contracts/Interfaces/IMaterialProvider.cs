using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Iis.Domain.Materials;
using Iis.Domain.MachineLearning;
using Iis.DataModel.Materials;
using Iis.Services.Contracts.Dtos;
using Iis.Services.Contracts.Params;
using Iis.Domain.Users;
using IIS.Services.Contracts.Materials;
using Iis.Interfaces.Elastic;

namespace IIS.Services.Contracts.Interfaces
{
    public interface IMaterialProvider
    {
        Task<Material> GetMaterialAsync(Guid id, User user);
        Task<Material[]> GetMaterialsByIdsAsync(ISet<Guid> ids, User user);
        Task<MaterialsDto> GetMaterialsAsync(Guid userId,
            string filterQuery,
            IReadOnlyCollection<Property> filteredItems,
            IReadOnlyCollection<string> cherryPickedItems,
            PaginationParams page,
            SortingParams sorting,
            CancellationToken ct = default);
        Task<IEnumerable<MaterialEntity>> GetMaterialEntitiesAsync();
        IReadOnlyCollection<MaterialSignEntity> GetMaterialSigns(string typeName);
        MaterialSign GetMaterialSign(string signValue);
        MaterialSign GetMaterialSign(Guid id);
        Task<List<MLResponse>> GetMLProcessingResultsAsync(Guid materialId);
        Task<MaterialsDto> GetMaterialsByImageAsync(Guid userId, PaginationParams page, string fileName, byte[] content);
        Task<(IEnumerable<Material> Materials, int Count)> GetMaterialsByNodeId(Guid nodeId);
        Task<(IEnumerable<Material> Materials, int Count)> GetMaterialsByNodeIdAndRelatedEntities(Guid nodeId);
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
        Task<IReadOnlyCollection<Guid>> GetMaterialsIdsAsync(int limit);
        Task<Material> GetMaterialAsync(Guid id);
        Task<IReadOnlyCollection<LocationHistoryDto>> GetLocationHistoriesAsync(Guid materialId);
    }
}
