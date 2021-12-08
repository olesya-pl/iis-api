using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Iis.Domain.Materials;
using Iis.Domain.MachineLearning;
using Iis.DataModel.Materials;
using Iis.Services.Contracts.Dtos;
using Iis.Domain.Users;
using IIS.Services.Contracts.Materials;
using Iis.Interfaces.Elastic;
using System.Linq.Expressions;
using Iis.Services.Contracts.Materials.Distribution;
using Iis.Interfaces.Common;
using Iis.Services.Contracts.Dtos.RadioElectronicSituation;

namespace IIS.Services.Contracts.Interfaces
{
    public interface IMaterialProvider
    {
        Task<Material[]> GetMaterialsByIdsAsync(ISet<Guid> ids, User user);
        Task<MaterialsDto> GetMaterialsAsync(Guid userId,
            string filterQuery,
            RelationsState? materialRelationsState,
            IReadOnlyCollection<Property> filteredItems,
            IReadOnlyCollection<string> cherryPickedItems,
            DateRange createdDateRange,
            PaginationParams page,
            SortingParams sorting,
            CancellationToken ct = default);
        Task<IEnumerable<MaterialEntity>> GetMaterialEntitiesAsync();
        IReadOnlyCollection<MaterialSignEntity> GetMaterialSigns(string typeName);
        MaterialSign GetMaterialSign(string signValue);
        Task<List<MLResponse>> GetMLProcessingResultsAsync(Guid materialId);
        Task<MaterialsDto> GetMaterialsByImageAsync(Guid userId, PaginationParams page, string fileName, byte[] content);
        Task<(IEnumerable<Material> Materials, int Count)> GetMaterialsByNodeId(Guid nodeId);
        Task<(IEnumerable<Material> Materials, int Count)> GetMaterialsByNodeIdAndRelatedEntities(Guid nodeId);
        Task<OutputCollection<Material>> GetMaterialsByNodeIdAsync(Guid nodeId, Guid userId, CancellationToken cancellationToken);
        Task<MaterialsDto> GetMaterialsCommonForEntitiesAsync(Guid userId,
            IEnumerable<Guid> nodeIdList,
            bool includeDescendants,
            string suggestion,
            DateRange createdDateRange,
            PaginationParams page,
            SortingParams sorting,
            CancellationToken ct = default);
        Task<Dictionary<Guid, int>> CountMaterialsByNodeIdSetAsync(ISet<Guid> nodeIdSet, Guid userId, CancellationToken cancellationToken);
        Task<List<MaterialsCountByType>> CountMaterialsByTypeAndNodeAsync(Guid nodeId);
        Task<(List<Material> Materials, int Count)> GetMaterialsByAssigneeIdAsync(Guid assigneeId);
        Task<(IEnumerable<Material> Materials,  int Count)> GetMaterialsLikeThisAsync(Guid userId, Guid materialId, PaginationParams page, SortingParams sorting);
        Task<bool> MaterialExists(Guid value);
        Task<IReadOnlyCollection<Guid>> GetMaterialsIdsAsync(int limit);
        Task<Material> GetMaterialAsync(Guid id);
        Task<Material> GetMaterialAsync(Guid id, User user);
        Task<IReadOnlyCollection<LocationHistoryDto>> GetLocationHistoriesAsync(Guid materialId);
        Task<IReadOnlyList<MaterialDistributionItem>> GetMaterialsForDistributionAsync(
            UserDistributionItem user,
            Expression<Func<MaterialEntity, bool>> filter);
        Task<IReadOnlyList<ResCallerReceiverDto>> GetCallInfoAsync(IReadOnlyList<Guid> nodeIds, CancellationToken cancellationToken = default);
    }
}